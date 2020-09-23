using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.UIInput;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia.Systems {

    public unsafe class LayoutSystem : IDisposable {

        internal Application application;

        internal TextSystem textSystem;
        internal ElementSystem elementSystem;

        // maybe this stuff should be per layout box and not per element. saves a lot of memory potentially since we only care about enabled stuff

        internal ElementTable<LayoutBoxUnion> layoutBoxTable;
        internal ElementTable<LayoutInfo> horizontalLayoutInfoTable;
        internal ElementTable<LayoutInfo> verticalLayoutInfoTable;
        internal ElementTable<PaddingBorderMargin> paddingBorderMarginTable;
        internal ElementTable<LayoutBoxInfo> layoutResultTable;
        internal ElementTable<LayoutHierarchyInfo> layoutHierarchyTable;
        internal ElementTable<TransformInfo> transformInfoTable;
        internal ElementTable<AlignmentInfo> alignmentInfoHorizontal;
        internal ElementTable<AlignmentInfo> alignmentInfoVertical;
        internal ElementTable<float4x4> localMatrices;
        internal ElementTable<float4x4> worldMatrices;
        internal ElementTable<ClipInfo> clipInfoTable;

        public DataList<Clipper>.Shared clippers;
        public DataList<float2>.Shared clipperIntersections;
        internal DataList<QueryResult>.Shared queryBuffer;

        private LightList<LayoutContext> layoutContexts;

        private int elementCapacity;
        private byte* layoutBackingStore;
        private byte* positionBackingStore;

        internal LayoutDataTables* tablePointers;

        internal DataList<ElementId>.Shared childrenChangedList;

        internal DataList<GridTrackSize> gridColTemplateBuffer;
        internal DataList<GridTrackSize> gridRowTemplateBuffer;
        internal DataList<GridTrackSize> gridColAutoSizeBuffer;
        internal DataList<GridTrackSize> gridRowAutoSizeBuffer;
        internal List_GridTrack.Shared gridColTrackBuffer;
        internal List_GridTrack.Shared gridRowTrackBuffer;
        internal DataList<ElementId> gridPlaceList;
        internal DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;

        internal DataList<ElementId>.Shared rootList;
        internal DataList<ElementId>.Shared ignoredLayoutList;
        internal DataList<ElementId>.Shared specialLayoutList;

        internal struct LayoutDataTables {

            public float4x4* localMatrices;
            public float4x4* worldMatrices;
            public LayoutBoxInfo* layoutBoxInfo;

        }

        public LayoutSystem(Application application, ElementSystem elementSystem, TextSystem textSystem) {
            this.application = application;
            this.textSystem = textSystem;
            this.elementSystem = elementSystem;
            this.layoutContexts = new LightList<LayoutContext>();
            this.tablePointers = TypedUnsafe.Malloc<LayoutDataTables>(Allocator.Persistent);
            this.gridPlaceList = new DataList<ElementId>(16, Allocator.Persistent);
            this.clippers = new DataList<Clipper>.Shared(16, Allocator.Persistent);
            this.clipperIntersections = new DataList<float2>.Shared(128, Allocator.Persistent);
            this.queryBuffer = new DataList<QueryResult>.Shared(16, Allocator.Persistent);
            this.clipperBoundsList = new DataList<AxisAlignedBounds2D>.Shared(16, Allocator.Persistent);
            this.rootList = new DataList<ElementId>.Shared(16, Allocator.Persistent);
            this.ignoredLayoutList = new DataList<ElementId>.Shared(16, Allocator.Persistent);
            this.specialLayoutList = new DataList<ElementId>.Shared(16, Allocator.Persistent);

            *tablePointers = default;
            ResizeBackingStore(application.InitialElementCapacity);
            worldMatrices.array[0] = float4x4.identity;

            this.childrenChangedList = new DataList<ElementId>.Shared(32, Allocator.Persistent);

//            application.onViewsSorted += uiViews => { runners.Sort((a, b) => Array.IndexOf(uiViews, b.rootElement.View) - Array.IndexOf(uiViews, a.rootElement.View)); };
        }

        // also triggered for destroy
        public void HandleElementDisabled(DataList<ElementId>.Shared disabledElements) {
            if (elementSystem.elementCapacity > elementCapacity) {
                ResizeBackingStore(elementSystem.elementCapacity);
            }

            for (int i = 0; i < disabledElements.size; i++) {
                // maybe only actually dispose if destroyed, keep disabled ones around?
                // styles are unlikely to change much
                // add to free list if using one and not 1-1 with elements
                ElementId elementId = disabledElements[i];
                // if is actually disabled -> dispose / unlink?
                
                ref LayoutBoxUnion layoutBox = ref layoutBoxTable[elementId];

                if (layoutBox.layoutType == LayoutBoxType.ScrollView) {
                    throw new NotImplementedException("Need to unregister scroll views, bruh");
                }

                layoutBox.Dispose();
            }

            rootList.size = 0;
            if (disabledElements.size > 1) {
                new FindHierarchyRootElements() {
                    elements = disabledElements,
                    traversalTable = elementSystem.traversalTable,
                    metaTable = elementSystem.metaTable,
                    roots = rootList,
                    mask = UIElementFlags.DisableRoot
                }.Run();
            }
            else {
                rootList.Add(disabledElements[0]);
            }

            // no need to burst this, usually only very few elements in this list 
            // cost of job running is probably larger than cost of running this in managed.
            for (int i = 0; i < rootList.size; i++) {
                ElementId elementId = rootList[i];

                // if element is at an index we haven't allocated yet, the memory for it is garbage.
                if (elementId.index >= elementCapacity) {
                    continue;
                }

                ref LayoutHierarchyInfo layoutInfo = ref layoutHierarchyTable[elementId];

                if (layoutInfo.behavior == LayoutBehavior.TranscludeChildren) {
                    // if was transcluded, need to remove all children from parent and mark that parent for layout. needs to handle multiple transclusion levels
                    LayoutUtil.Untransclude(elementId, elementSystem.traversalTable, elementSystem.hierarchyTable, layoutHierarchyTable);
                    LayoutUtil.UnlinkFromParent(elementId, layoutHierarchyTable);
                }
                else if (layoutInfo.behavior == LayoutBehavior.Normal) {
                    LayoutUtil.UnlinkFromParent(elementId, layoutHierarchyTable);
                }

                if (layoutInfo.parentId != default) {
                    childrenChangedList.Add(layoutInfo.parentId);
                }

                layoutInfo = default;
            }
        }

        // also triggered for elements created this frame
        public void HandleElementEnabled(DataList<ElementId>.Shared enabledElements) {
            if (elementSystem.elementCapacity > elementCapacity) {
                ResizeBackingStore(elementSystem.elementCapacity);
            }

            for (int i = 0; i < enabledElements.size; i++) {
                UIElement element = elementSystem.instanceTable[enabledElements[i].index];
                // this point styles are all final for the frame because we ignore changesets for elements enabled this frame
                ref LayoutHierarchyInfo hierarchyInfo = ref layoutHierarchyTable[element.id];
                ref LayoutBoxUnion layoutBox = ref layoutBoxTable[element.id];
                ref LayoutInfo horizontalLayoutInfo = ref horizontalLayoutInfoTable[element.id];
                ref LayoutInfo verticalLayoutInfo = ref verticalLayoutInfoTable[element.id];
                ref ClipInfo clipInfo = ref clipInfoTable[element.id];
                ref ElementTraversalInfo traversalInfo = ref elementSystem.traversalTable[element.id];

                hierarchyInfo = default;

                UIStyleSet style = element.style;
                
                // todo -- remove shitty hack
                // basically we need to differentiate between init styles from the proxy target (scroll view is currently the only example) and the elements own styles.
                // in the proxy case we want to treat the actual target element as a dummy, and read layout styles from the parent
                if (element.parent is ScrollView && element.siblingIndex == 0) {
                    layoutBox.Initialize(LayoutBoxUnion.GetLayoutBoxType(element), this, element, element.parent);
                }
                else {
                    layoutBox.Initialize(LayoutBoxUnion.GetLayoutBoxType(element), this, element, element);
                }

                horizontalLayoutInfo.requiresLayout = true;
                verticalLayoutInfo.requiresLayout = true;
                
                hierarchyInfo.behavior = style.LayoutBehavior;

                // todo -- maybe skip for transclusion since we never do the layout
                if (hierarchyInfo.behavior == LayoutBehavior.TranscludeChildren) {
                    GetLayoutContext(element.View).transclusionCount++;
                }

                traversalInfo.zIndex = element.style.ZIndex;

                transformInfoTable[element.id] = new TransformInfo() {
                    positionX = style.TransformPositionX,
                    positionY = style.TransformPositionY,
                    scaleX = style.TransformScaleX,
                    scaleY = style.TransformScaleY,
                    rotation = style.TransformRotation,
                    pivotX = style.TransformPivotX,
                    pivotY = style.TransformPivotY
                };

                // only create if needed, allocate an id. how do I recycle it? when pooling the box probably
                alignmentInfoHorizontal[element.id] = new AlignmentInfo() {
                    origin = style.AlignmentOriginX,
                    offset = style.AlignmentOffsetX,
                    direction = style.AlignmentDirectionX,
                    target = style.AlignmentTargetX,
                    boundary = style.AlignmentBoundaryX
                };

                // only create if needed, allocate an id
                alignmentInfoVertical[element.id] = new AlignmentInfo() {
                    origin = style.AlignmentOriginY,
                    offset = style.AlignmentOffsetY,
                    direction = style.AlignmentDirectionY,
                    target = style.AlignmentTargetY,
                    boundary = style.AlignmentBoundaryY
                };

                ref PaddingBorderMargin paddingBorderMargin = ref paddingBorderMarginTable[element.id];

                paddingBorderMargin.marginTop = style.MarginTop;
                paddingBorderMargin.marginRight = style.MarginRight;
                paddingBorderMargin.marginBottom = style.MarginBottom;
                paddingBorderMargin.marginLeft = style.MarginLeft;

                paddingBorderMargin.borderTop = style.BorderTop;
                paddingBorderMargin.borderRight = style.BorderRight;
                paddingBorderMargin.borderBottom = style.BorderBottom;
                paddingBorderMargin.borderLeft = style.BorderLeft;

                paddingBorderMargin.paddingTop = style.PaddingTop;
                paddingBorderMargin.paddingRight = style.PaddingRight;
                paddingBorderMargin.paddingBottom = style.PaddingBottom;
                paddingBorderMargin.paddingLeft = style.PaddingLeft;

                horizontalLayoutInfo.fit = style.LayoutFitHorizontal;
                horizontalLayoutInfo.prefSize = style.PreferredWidth;
                horizontalLayoutInfo.minSize = style.MinWidth;
                horizontalLayoutInfo.maxSize = style.MaxWidth;

                Texture bgTexture = style.BackgroundImage;
                if (!ReferenceEquals(bgTexture, null)) {
                    UIFixedLength minX = style.BackgroundRectMinX;
                    UIFixedLength maxX = style.BackgroundRectMaxX;
                    // em and view size go to 0
                    int resolvedMin = (int) MeasurementUtil.ResolveFixedSize(bgTexture.width, default, default, minX);
                    int resolvedMax = (int) MeasurementUtil.ResolveFixedSize(bgTexture.width, default, default, maxX);
                    horizontalLayoutInfo.bgSize = Mathf.Clamp(resolvedMax - resolvedMin, 0, bgTexture.width);
                }
                else {
                    horizontalLayoutInfo.bgSize = 0;
                }

                horizontalLayoutInfo.finalSize = -1;
                horizontalLayoutInfo.parentBlockSize = default;
                horizontalLayoutInfo.isBlockProvider = !horizontalLayoutInfo.prefSize.IsContentBased;

                verticalLayoutInfo.fit = style.LayoutFitVertical;
                verticalLayoutInfo.prefSize = style.PreferredHeight;
                verticalLayoutInfo.minSize = style.MinHeight;
                verticalLayoutInfo.maxSize = style.MaxHeight;
                if (!ReferenceEquals(bgTexture, null)) {
                    UIFixedLength minY = style.BackgroundRectMinY;
                    UIFixedLength maxY = style.BackgroundRectMaxY;
                    // em and view size go to 0
                    int resolvedMin = (int) MeasurementUtil.ResolveFixedSize(bgTexture.height, default, default, minY);
                    int resolvedMax = (int) MeasurementUtil.ResolveFixedSize(bgTexture.height, default, default, maxY);
                    verticalLayoutInfo.bgSize = Mathf.Clamp(resolvedMax - resolvedMin, 0, bgTexture.height);
                }
                else {
                    verticalLayoutInfo.bgSize = 0;
                }

                verticalLayoutInfo.finalSize = -1;
                verticalLayoutInfo.parentBlockSize = default;
                verticalLayoutInfo.isBlockProvider = !verticalLayoutInfo.prefSize.IsContentBased;

                clipInfo.overflow = style.OverflowX;
                clipInfo.visibility = style.Visibility;
                clipInfo.pointerEvents = style.PointerEvents;
                clipInfo.clipBehavior = style.ClipBehavior;
                clipInfo.clipBounds = style.ClipBounds;
                clipInfo.isMouseQueryHandler = element is IPointerQueryHandler;
            }

            rootList.size = 0;
            ignoredLayoutList.size = 0;

            if (enabledElements.size > 1) {
                new FindHierarchyRootElements() {
                    elements = enabledElements,
                    traversalTable = elementSystem.traversalTable,
                    metaTable = elementSystem.metaTable,
                    roots = rootList,
                    mask = UIElementFlags.EnabledRoot
                }.Run();
            }
            else {
                rootList.Add(enabledElements[0]);
            }

            new HierarchyBuildJob() {
                roots = rootList,
                layoutIgnoredList = ignoredLayoutList,
                specialList = specialLayoutList,
                hierarchyTable = elementSystem.hierarchyTable,
                metaTable = elementSystem.metaTable,
                layoutHierarchyTable = layoutHierarchyTable,
            }.Run();
            
            // could technically be done in parallel.

            // ignored elements are still in this list
            for (int i = 0; i < enabledElements.size; i++) {
                ElementId elementId = enabledElements[i];

                ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutHierarchyTable[elementId];
                // this feels out of place
                layoutResultTable[elementId].layoutParentId = layoutHierarchyInfo.parentId;
                layoutResultTable[elementId].scrollValues = null;

                if (layoutHierarchyInfo.behavior != LayoutBehavior.TranscludeChildren) {
                    layoutBoxTable[elementId].OnChildrenChanged(this);
                }
            }

            for (int i = 0; i < rootList.size; i++) {
                ElementId elementId = rootList[i];
                ref LayoutHierarchyInfo layoutInfo = ref layoutHierarchyTable[elementId];
                switch (layoutInfo.behavior) {
                    case LayoutBehavior.Normal:
                        ElementId parentId = LayoutUtil.FindLayoutParent(elementId, elementSystem.hierarchyTable, layoutHierarchyTable);
                        LayoutUtil.Insert(parentId, elementId, elementSystem.traversalTable, layoutHierarchyTable);
                        childrenChangedList.Add(layoutInfo.parentId);
                        break;

                    case LayoutBehavior.Ignored:
                        UIElement element = elementSystem.instanceTable[elementId.index];
                        GetLayoutContext(element.View).AddToIgnoredList(element.id);
                        break;

                    case LayoutBehavior.TranscludeChildren:
                        childrenChangedList.Add(layoutInfo.parentId);
                        LayoutUtil.TranscludeUnattached(elementId, layoutHierarchyTable, elementSystem.traversalTable);
                        break;
                }
            }

            for (int i = 0; i < ignoredLayoutList.size; i++) {
                UIElement element = elementSystem.instanceTable[ignoredLayoutList[i].index];
                GetLayoutContext(element.View).AddToIgnoredList(element.id);
            }
        }

        internal LayoutContext GetLayoutContext(UIView view) {
            for (int i = 0; i < layoutContexts.size; i++) {
                if (layoutContexts[i].view == view) {
                    return layoutContexts[i];
                }
            }

            return null;
        }

        internal LayoutContext GetLayoutContext(int viewId) {
            for (int i = 0; i < layoutContexts.size; i++) {
                if (layoutContexts[i].view.id == viewId) {
                    return layoutContexts[i];
                }
            }

            return null;
        }
        // only called for elements that were not enabled this frame
        // todo -- totally burstable when styles are blittable
        public void HandleStylePropertyUpdates(UIElement element, StyleProperty[] properties, int propertyCount) {
            textSystem.HandleStyleChanged(element, properties, propertyCount);
            ref LayoutInfo horizontalLayoutInfo = ref horizontalLayoutInfoTable[element.id];
            ref LayoutInfo verticalLayoutInfo = ref verticalLayoutInfoTable[element.id];
            ref PaddingBorderMargin layoutPropertyEntry = ref paddingBorderMarginTable[element.id];
            ref AlignmentInfo horizontalAlignmentInfo = ref alignmentInfoHorizontal[element.id];
            ref AlignmentInfo verticalAlignmentInfo = ref alignmentInfoVertical[element.id];

            bool recomputeBGSize = false;

            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.PreferredWidth:
                        horizontalLayoutInfo.prefSize = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MaxWidth:
                        horizontalLayoutInfo.maxSize = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MinWidth:
                        horizontalLayoutInfo.minSize = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.PreferredHeight:
                        verticalLayoutInfo.prefSize = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MaxHeight:
                        verticalLayoutInfo.maxSize = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MinHeight:
                        verticalLayoutInfo.minSize = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.AlignmentBoundaryX:
                        horizontalAlignmentInfo.boundary = property.AsAlignmentBoundary;
                        break;

                    case StylePropertyId.AlignmentTargetX:
                        horizontalAlignmentInfo.target = property.AsAlignmentTarget;
                        break;

                    case StylePropertyId.AlignmentOriginX:
                        horizontalAlignmentInfo.origin = property.AsOffsetMeasurement;
                        break;

                    case StylePropertyId.AlignmentOffsetX:
                        horizontalAlignmentInfo.offset = property.AsOffsetMeasurement;
                        break;

                    case StylePropertyId.AlignmentDirectionX:
                        horizontalAlignmentInfo.direction = property.AsAlignmentDirection;
                        break;

                    case StylePropertyId.AlignmentBoundaryY:
                        verticalAlignmentInfo.boundary = property.AsAlignmentBoundary;
                        break;

                    case StylePropertyId.AlignmentTargetY:
                        verticalAlignmentInfo.target = property.AsAlignmentTarget;
                        break;

                    case StylePropertyId.AlignmentOriginY:
                        verticalAlignmentInfo.origin = property.AsOffsetMeasurement;
                        break;

                    case StylePropertyId.AlignmentOffsetY:
                        verticalAlignmentInfo.offset = property.AsOffsetMeasurement;
                        break;

                    case StylePropertyId.AlignmentDirectionY:
                        verticalAlignmentInfo.direction = property.AsAlignmentDirection;
                        break;

                    case StylePropertyId.TransformRotation:
                        transformInfoTable[element.id].rotation = property.AsFloat;
                        break;

                    case StylePropertyId.TransformPositionX:
                        transformInfoTable[element.id].positionX = property.AsOffsetMeasurement;
                        break;

                    case StylePropertyId.TransformPositionY:
                        transformInfoTable[element.id].positionY = property.AsOffsetMeasurement;
                        break;

                    case StylePropertyId.TransformPivotX:
                        transformInfoTable[element.id].pivotX = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.TransformPivotY:
                        transformInfoTable[element.id].pivotY = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.TransformScaleX:
                        transformInfoTable[element.id].scaleX = property.AsFloat;
                        break;

                    case StylePropertyId.TransformScaleY:
                        transformInfoTable[element.id].scaleY = property.AsFloat;
                        break;

                    case StylePropertyId.MarginTop:
                        layoutPropertyEntry.marginTop = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.MarginRight:
                        layoutPropertyEntry.marginRight = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.MarginBottom:
                        layoutPropertyEntry.marginBottom = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.MarginLeft:
                        layoutPropertyEntry.marginLeft = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderTop:
                        layoutPropertyEntry.borderTop = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRight:
                        layoutPropertyEntry.borderRight = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderBottom:
                        layoutPropertyEntry.borderRight = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderLeft:
                        layoutPropertyEntry.borderLeft = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.PaddingTop:
                        layoutPropertyEntry.paddingTop = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.PaddingRight:
                        layoutPropertyEntry.paddingRight = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.PaddingBottom:
                        layoutPropertyEntry.paddingBottom = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.PaddingLeft:
                        layoutPropertyEntry.paddingBottom = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.Visibility:
                        clipInfoTable[element.id].visibility = property.AsVisibility;
                        break;

                    case StylePropertyId.ZIndex:
                        elementSystem.traversalTable[element.id].zIndex = element.style.ZIndex;
                        break;

                    case StylePropertyId.BackgroundRectMinX:
                    case StylePropertyId.BackgroundRectMaxX:
                    case StylePropertyId.BackgroundRectMinY:
                    case StylePropertyId.BackgroundRectMaxY:
                    case StylePropertyId.BackgroundImage:
                        recomputeBGSize = true;
                        break;

                    case StylePropertyId.LayoutBehavior:
                        LayoutBehavior previousBehavior = layoutHierarchyTable[element.id].behavior;
                        LayoutBehavior newBehavior = property.AsLayoutBehavior;

                        if (previousBehavior == newBehavior) {
                            continue;
                        }

                        ElementId p = LayoutUtil.FindLayoutParent(element.id, elementSystem.hierarchyTable, layoutHierarchyTable);

                        if (previousBehavior == LayoutBehavior.Ignored) {
                            GetLayoutContext(element.View).RemoveFromIgnoredList(element.id);
                            LayoutUtil.Insert(p, element.id, elementSystem.traversalTable, layoutHierarchyTable);
                        }
                        else if (previousBehavior == LayoutBehavior.TranscludeChildren) {
                            LayoutUtil.Untransclude(element.id, elementSystem.traversalTable, elementSystem.hierarchyTable, layoutHierarchyTable);
                            GetLayoutContext(element.View).transclusionCount--;
                            p = layoutHierarchyTable[element.id].parentId;
                            // childrenChangedList.Add(element.id);
                            childrenChangedList.Add(element.id);
                        }

                        if (newBehavior == LayoutBehavior.Ignored) {
                            GetLayoutContext(element.View).AddToIgnoredList(element.id);
                            LayoutUtil.UnlinkFromParent(element.id, layoutHierarchyTable);
                        }
                        else if (newBehavior == LayoutBehavior.TranscludeChildren) {
                            LayoutUtil.Transclude(element.id, layoutHierarchyTable);
                            layoutResultTable[element.id] = default;
                            GetLayoutContext(element.View).transclusionCount++;
                        }

                        childrenChangedList.Add(p);
                        // childrenChangedList.Add(p);

                        layoutHierarchyTable[element.id].behavior = newBehavior;
                        break;
                }
            }

            if (recomputeBGSize) {
                Size bgSize = UpdateTextureSize(element);
                horizontalLayoutInfo.bgSize = bgSize.width;
                verticalLayoutInfo.bgSize = bgSize.height;
            }

            ref LayoutBoxUnion box = ref layoutBoxTable[element.id];
            if (box.layoutType == LayoutBoxType.ScrollView) {
                
            }
            else {
                layoutBoxTable[element.id].OnStylePropertiesChanged(this, element, properties, propertyCount);
            }

            ElementId parentId = layoutHierarchyTable[element.id].parentId;
            if (parentId != default) {
                layoutBoxTable[parentId].OnChildStyleChanged(this, element.id, properties, propertyCount);
            }
        }

        internal static Size UpdateTextureSize(UIElement element) {
            Texture bgTexture = element.style.BackgroundImage;
            if (!ReferenceEquals(bgTexture, null)) {
                UIFixedLength minX = element.style.BackgroundRectMinX;
                UIFixedLength maxX = element.style.BackgroundRectMaxX;
                UIFixedLength minY = element.style.BackgroundRectMinY;
                UIFixedLength maxY = element.style.BackgroundRectMaxY;
                // em and view size go to 0
                int resolvedMinX = (int) MeasurementUtil.ResolveFixedSize(bgTexture.width, default, default, minX);
                int resolvedMaxX = (int) MeasurementUtil.ResolveFixedSize(bgTexture.width, default, default, maxX);
                // em and view size go to 0
                int resolvedMinY = (int) MeasurementUtil.ResolveFixedSize(bgTexture.height, default, default, minY);
                int resolvedMaxY = (int) MeasurementUtil.ResolveFixedSize(bgTexture.height, default, default, maxY);
                return new Size(
                    Mathf.Clamp(resolvedMaxX - resolvedMinX, 0, bgTexture.width),
                    Mathf.Clamp(resolvedMaxY - resolvedMinY, 0, bgTexture.height)
                );
            }

            return default;
        }

        public void MarkForChildrenUpdate(ElementId id) {
            if ((elementSystem.metaTable[id].flags & UIElementFlags.EnableStateChanged) == 0) {
                childrenChangedList.Add(id);
            }
        }

        public void RunLayout() {
            // cannot be parallel atm. can be a job though
            for (int i = 0; i < gridPlaceList.size; i++) {
                ref LayoutBoxUnion box = ref layoutBoxTable[gridPlaceList[i]];
                if (box.layoutType == LayoutBoxType.Image) {
                    if (box.image.layoutBox->layoutType == LayoutBoxType.Grid) {
                        box.image.layoutBox->grid.RunPlacement(this);
                    }
                }
                else if (box.layoutType == LayoutBoxType.Grid) {
                    box.grid.RunPlacement(this);
                }
            }

            gridPlaceList.size = 0;

            if (childrenChangedList.size > 1) {
                new RemoveListDuplicates() {
                    list = childrenChangedList
                }.Run();
            }

            for (int i = 0; i < childrenChangedList.size; i++) {
                layoutBoxTable[childrenChangedList[i]].OnChildrenChanged(this);
            }

            childrenChangedList.size = 0;

            float applicationWidth = application.Width;
            float applicationHeight = application.Height;
            NativeArray<JobHandle> layoutHandles = new NativeArray<JobHandle>(layoutContexts.size, Allocator.Temp);
            NativeArray<JobHandle> verticalAlignHandles = new NativeArray<JobHandle>(layoutContexts.size, Allocator.Temp);

            clippers.size = 0;

            // never
            clippers.Add(new Clipper() {
                aabb = new AxisAlignedBounds2D(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue),
                intersectionRange = new RangeInt(0, 4)
            });

            // screen
            clippers.Add(new Clipper() {
                // todo -- maybe not the right value since unity likes to change the screen size randomly with focus 
                aabb = new AxisAlignedBounds2D(0, 0, Screen.width, Screen.height),
                parentIndex = 0,
                intersectionRange = new RangeInt(4, 4),
                isCulled = false
            });

            clipperIntersections.SetSize(8);
            clipperIntersections[0] = new float2(0, 0);
            clipperIntersections[1] = new float2(99999, 0);
            clipperIntersections[2] = new float2(99999, 99999);
            clipperIntersections[3] = new float2(0, 99999);

            clipperIntersections[4] = new float2(0, 0);
            clipperIntersections[5] = new float2(Screen.width, 0);
            clipperIntersections[6] = new float2(Screen.width, Screen.height);
            clipperIntersections[7] = new float2(0, Screen.height);
            
            JobHandle constructClippers = new ConstructClippersJob() {
                clipInfoTable = clipInfoTable,
                clipperOutputList = clippers,
                layoutHierarchyTable = layoutHierarchyTable,
                viewRootIds = application.viewRootIds
            }.Schedule();

            for (int i = 0; i < layoutContexts.size; i++) {
                ref LayoutContext layoutContext = ref layoutContexts.array[i];

                UIView view = layoutContexts[i].view;
                ElementId viewRootId = view.dummyRoot.id;
                int activeElementCount = layoutContext.ActiveElementCount;

                ViewParameters viewParameters = new ViewParameters() {
                    viewX = view.position.x,
                    viewY = view.position.y,
                    viewWidth = view.Viewport.width,
                    viewHeight = view.Viewport.height,
                    applicationWidth = applicationWidth,
                    applicationHeight = applicationHeight
                };

                layoutResultTable[viewRootId].actualSize = new float2(viewParameters.viewWidth, viewParameters.viewHeight);

                layoutContext.elementList.EnsureCapacity(activeElementCount);
                layoutContext.parentList.EnsureCapacity(activeElementCount);
                layoutContext.elementList.size = 0;
                layoutContext.parentList.size = 0;

                layoutContext.runner->lineInfoBuffer = layoutContext.lineBuffer;
                layoutContext.runner->layoutHierarchyTable = layoutHierarchyTable.array;
                layoutContext.runner->layoutBoxInfoTable = layoutResultTable.array;
                layoutContext.runner->viewParameters = viewParameters;
                layoutContext.runner->layoutBoxTable = layoutBoxTable.array;
                layoutContext.runner->horizontalLayoutInfoTable = horizontalLayoutInfoTable.array;
                layoutContext.runner->verticalLayoutInfoTable = verticalLayoutInfoTable.array;
                layoutContext.runner->emTable = elementSystem.emTable.array;
                layoutContext.runner->fontAssetMap = application.ResourceManager.fontAssetMap.GetArrayPointer();

                // dont expect lots and lots of text updates per frame, a few dozen at most
                // still nice to be parallel
                // something needs to own the text data then
                // per-view text system?
                // give data to layout context?
                // kind of is a layout thing

                JobHandle emTableHandle = new UpdateEmTable() {
                    rootId = viewRootId,
                    activeElementCount = activeElementCount,
                    emTable = elementSystem.emTable,
                    hierarchyTable = elementSystem.hierarchyTable,
                    metaTable = elementSystem.metaTable,
                    viewWidth = viewParameters.viewWidth,
                    viewHeight = viewParameters.viewHeight
                }.Schedule();

                textSystem.GetTextChangesForView(viewRootId, layoutContext.textChangeBuffer);

                JobHandle textTransformUpdates = new TextSystem.UpdateTextTransformJob() {
                    changedElementIds = layoutContext.textChangeBuffer,
                }.Schedule();

                JobHandle textLayoutUpdates = UIForiaScheduler.Await(emTableHandle, textTransformUpdates).ThenParallel(new UpdateTextLayoutJob() {
                    parallel = new ParallelParams(layoutContext.textChangeBuffer.size, 10),
                    viewportWidth = viewParameters.viewWidth,
                    viewportHeight = viewParameters.viewHeight,
                    emTable = elementSystem.emTable,
                    textChanges = layoutContext.textChangeBuffer,
                    fontAssetMap = application.ResourceManager.fontAssetMap,
                });

                JobHandle flatten = default;
                new FlattenLayoutTree() {
                    viewRootId = viewRootId,
                    metaTable = elementSystem.metaTable,
                    elementList = layoutContext.elementList,
                    parentList = layoutContext.parentList,
                    ignoredList = layoutContext.ignoredList,
                    traversalTable = elementSystem.traversalTable,
                    layoutHierarchyTable = layoutHierarchyTable,
                    viewActiveElementCount = activeElementCount
                }.Run();

                // parallel for large views
                JobHandle updatePaddingMarginBorder = UIForiaScheduler.Await(emTableHandle, flatten).Then(new UpdatePaddingBorderMargin() {
                    parallel = new ParallelParams(layoutContext.elementList.size, 250),
                    elementList = layoutContext.elementList,
                    emTable = elementSystem.emTable,
                    propertyTable = paddingBorderMarginTable,
                    verticalLayoutInfo = verticalLayoutInfoTable,
                    horizontalLayoutInfo = horizontalLayoutInfoTable,
                    layoutResultTable = layoutResultTable,
                    viewParameters = viewParameters
                });

                // possible to be parallel for big tree with ignored, fixed, and no fit
                JobHandle horizontalLayout = UIForiaScheduler.Await(textLayoutUpdates, updatePaddingMarginBorder).Then(new RunLayoutHorizontal() {
                    runner = layoutContext.runner,
                    elementList = layoutContext.elementList,
                    layoutBoxTable = layoutBoxTable
                });

                // parallel for big list
                JobHandle horizontalAlignment = UIForiaScheduler.Await(horizontalLayout).ThenParallel(new ApplyHorizontalAlignments() {
                    parallel = new ParallelParams(layoutContext.elementList.size, 250),
                    alignmentTable = alignmentInfoHorizontal,
                    elementList = layoutContext.elementList,
                    mousePosition = application.inputSystem.MousePosition.x,
                    viewParameters = viewParameters,
                    viewRootId = viewRootId,
                    layoutBoxInfoTable = layoutResultTable
                });

                // possible to be parallel for big tree with ignored, fixed, and no fit
                JobHandle verticalLayout = UIForiaScheduler.Await(horizontalLayout).Then(new RunLayoutVertical() {
                    runner = layoutContext.runner,
                    elementList = layoutContext.elementList,
                    layoutBoxTable = layoutBoxTable
                });

                // todo -- move
                JobHandle scrollVertical = UIForiaScheduler.Await(verticalLayout).Then(new UpdateScrollVertical() {
                    runner = layoutContext.runner,
                    scrollBoxHead = scrollBoxHead,
                    layoutBoxTable = layoutBoxTable
                });

                // parallel for big list
                JobHandle verticalAlignment = UIForiaScheduler.Await(scrollVertical, verticalLayout).ThenParallel(new ApplyVerticalAlignments() {
                    parallel = new ParallelParams(layoutContext.elementList.size, 250),
                    alignmentTable = alignmentInfoVertical,
                    elementList = layoutContext.elementList,
                    mousePosition = application.inputSystem.MousePosition.y,
                    viewParameters = viewParameters,
                    viewRootId = viewRootId,
                    layoutBoxInfoTable = layoutResultTable
                });
                verticalAlignHandles[i] = verticalAlignment;

                // parallel if big list element count, single if small, order doesnt matter
                JobHandle buildLocalMatrices = UIForiaScheduler.Await(horizontalAlignment, verticalAlignment).ThenParallel(new BuildLocalMatrices() {
                    parallel = new ParallelParams(layoutContext.elementList.size, 250),
                    elementList = layoutContext.elementList,
                    localMatrices = localMatrices,
                    parentList = layoutContext.parentList,
                    viewParameters = viewParameters,
                    transformInfoTable = transformInfoTable,
                    layoutBoxInfoTable = layoutResultTable
                });

                // todo -- merge with local matrix building, its likely much faster
                JobHandle buildWorldMatrices = UIForiaScheduler.Await(buildLocalMatrices).Then(new BuildWorldMatrices() {
                    elementList = layoutContext.elementList,
                    localMatrices = localMatrices,
                    parentList = layoutContext.parentList,
                    worldMatrices = worldMatrices
                });

                // todo -- merge with matrix building, its likely much faster
                layoutHandles[i] = UIForiaScheduler.Await(buildWorldMatrices).ThenParallel(new ComputeOrientedBounds() {
                    parallel = new ParallelParams(layoutContext.elementList.size, 250),
                    elementList = layoutContext.elementList,
                    worldMatrices = worldMatrices,
                    clipInfoTable = clipInfoTable,
                    layoutResultTable = layoutResultTable
                });

                JobHandle.ScheduleBatchedJobs();
            }

            JobHandle layoutCompleted = JobHandle.CombineDependencies(layoutHandles);

            // todo -- this can be parallel if we find the view indices and then count how many were added per view to get the ranges
            // todo -- can start this while we compute local matrices if this job re-computes local and world matrices, big savings!
            JobHandle updateClippers = UIForiaScheduler.Await(constructClippers, layoutCompleted, JobHandle.CombineDependencies(verticalAlignHandles)).Then(new UpdateClippers() {
                clipperList = clippers,
                intersectionResults = clipperIntersections,
                clipInfoTable = clipInfoTable,
                clipperBoundsList = clipperBoundsList,
                layoutResultTable = layoutResultTable
            });

            NativeArray<JobHandle> cullingHandles = new NativeArray<JobHandle>(layoutContexts.size, Allocator.Temp);

            for (int i = 0; i < layoutContexts.size; i++) {
                cullingHandles[i] = UIForiaScheduler.Await(updateClippers, layoutCompleted).ThenParallel(new ComputeElementCulling() {
                    parallel = new ParallelParams(layoutContexts[i].elementList.size, 250),
                    clipperList = clippers,
                    layoutResultTable = layoutResultTable,
                    elementList = layoutContexts[i].elementList,
                    clipInfoTable = clipInfoTable
                });
            }

            JobHandle.CompleteAll(cullingHandles);

            // todo -- find a better place for this stuff to happen
            new UpdateTextRenderRanges() {
                fontAssetMap = application.ResourceManager.fontAssetMap,
                rangeSizeLimit = 200,
                activeTextElementInfos = textSystem.activeTextElementInfo
            }.Run();

            new UpdateTextMaterialBuffersJob() {
                activeTextElementIds = textSystem.activeTextElementInfo
            }.Run();

            textSystem.UpdateEffects();

            new UpdateTextRenderBounds() {
                activeTextElementInfos = textSystem.activeTextElementInfo,
                fontAssetMap = application.ResourceManager.fontAssetMap
            }.Run();

            cullingHandles.Dispose();
            layoutHandles.Dispose();
            verticalAlignHandles.Dispose();
        }

        public void OnViewAdded(UIView view) {
            layoutContexts.Add(new LayoutContext(view, this));
        }

        // todo -- re-instate this
        public void OnViewRemoved(UIView view) { }

        private static DataList<GridTrackSize> BufferGridTemplate(ref DataList<GridTrackSize> trackList, IReadOnlyList<GridTrackSize> template) {
            if (trackList.GetArrayPointer() == null) {
                trackList = new DataList<GridTrackSize>(Mathf.Max(8, template.Count * 2), Allocator.Persistent);
            }

            trackList.SetSize(template.Count);
            for (int i = 0; i < template.Count; i++) {
                trackList[i] = template[i];
            }

            return trackList;
        }

        public DataList<GridTrackSize> BufferGridColTemplate(IReadOnlyList<GridTrackSize> template) {
            return BufferGridTemplate(ref gridColTemplateBuffer, template);
        }

        public DataList<GridTrackSize> BufferGridRowTemplate(IReadOnlyList<GridTrackSize> template) {
            return BufferGridTemplate(ref gridRowTemplateBuffer, template);
        }

        public DataList<GridTrackSize> BufferGridColAutoSize(IReadOnlyList<GridTrackSize> template) {
            return BufferGridTemplate(ref gridColAutoSizeBuffer, template);
        }

        public DataList<GridTrackSize> BufferGridRowAutoSize(IReadOnlyList<GridTrackSize> template) {
            return BufferGridTemplate(ref gridRowAutoSizeBuffer, template);
        }

        internal List_GridTrack.Shared GetGridColTrackBuffer() {
            if (gridColTrackBuffer.state == null) {
                gridColTrackBuffer = new List_GridTrack.Shared(16, Allocator.Persistent);
            }

            return gridColTrackBuffer;
        }

        internal List_GridTrack.Shared GetGridRowTrackBuffer() {
            if (gridRowTrackBuffer.state == null) {
                gridRowTrackBuffer = new List_GridTrack.Shared(16, Allocator.Persistent);
            }

            return gridRowTrackBuffer;
        }

        public void EnqueueGridForPlacement(ElementId elementId) {
            for (int i = 0; i < gridPlaceList.size; i++) {
                if (gridPlaceList[i] == elementId) {
                    return;
                }
            }

            gridPlaceList.Add(elementId);
        }

        // don't want a list of members here since id only ever use it for this query function

        internal struct QueryResult {

            public ElementId elementId;
            public bool requiresCustomHandling;

        }

        public void QueryPoint(float2 point, IList<UIElement> retn) {
            if (!new Rect(0, 0, application.Width, application.Height).Contains(point)) {
                return;
            }

            queryBuffer.size = 0;

            for (int i = 0; i < layoutContexts.size; i++) {
                new QueryPointJob() {
                    clippers = clippers,
                    point = point,
                    retn = queryBuffer,
                    clipInfoTable = clipInfoTable,
                    elementList = layoutContexts[i].elementList,
                    viewRootId = layoutContexts[i].view.dummyRoot.id
                }.Run();
            }

            for (int i = 0; i < queryBuffer.size; i++) {
                UIElement instance = elementSystem.instanceTable[queryBuffer[i].elementId.index];

                if (queryBuffer[i].requiresCustomHandling) {
                    if (instance is IPointerQueryHandler handler && handler.ContainsPoint(point)) {
                        retn.Add(instance);
                    }
                }
                else {
                    retn.Add(instance);
                }
            }
        }

        [BurstCompile]
        internal struct QueryPointJob : IJob {

            public float2 point;
            public DataList<Clipper>.Shared clippers;
            public DataList<QueryResult>.Shared retn;
            public ElementTable<ClipInfo> clipInfoTable;
            public DataList<ElementId>.Shared elementList;
            public ElementId viewRootId;

            public void Execute() {
                if (clippers.size < 2) return;
                DataList<bool> containsPoint = new DataList<bool>(clippers.size, Allocator.Temp);
                containsPoint[0] = true; // never clipper
                containsPoint[1] = true; // screen clipper

                int viewStart = 2;
                int viewEnd = clippers.size;

                for (int i = 2; i < clippers.size; i++) {
                    if (clippers[i].elementId == viewRootId) {
                        viewStart = i;
                        viewEnd = i + clippers[i].subClipperCount;
                        break;
                    }
                }

                for (int i = viewStart; i < viewEnd; i++) {
                    ClipInfo clipInfo = clipInfoTable[clippers[i].elementId];
                    containsPoint[i] = !clippers[i].isCulled && PolygonUtil.PointInOrientedBounds(point, clipInfo.orientedBounds);
                }

                for (int i = 0; i < elementList.size; i++) {
                    ClipInfo clipInfo = clipInfoTable[elementList[i]];
                    Clipper clipper = clippers[clipInfo.clipperIndex];

                    if (clipper.isCulled || !containsPoint[clipInfo.clipperIndex] || clipInfo.visibility == Visibility.Hidden || clipInfo.pointerEvents == PointerEvents.None) {
                        continue;
                    }

                    if (clipInfo.isMouseQueryHandler) {
                        retn.Add(new QueryResult() {elementId = elementList[i], requiresCustomHandling = true});
                        continue;
                    }

                    if (PolygonUtil.PointInOrientedBounds(point, clipInfo.orientedBounds)) {
                        retn.Add(new QueryResult() {elementId = elementList[i]});
                    }
                }

                containsPoint.Dispose();
            }

        }

        public void Dispose() {
            for (int i = 0; i < elementCapacity; i++) {
                if (layoutBoxTable.array[i].layoutType != LayoutBoxType.Unset) {
                    layoutBoxTable.array[i].Dispose();
                }
            }

            TypedUnsafe.Dispose(layoutBackingStore, Allocator.Persistent);
            TypedUnsafe.Dispose(positionBackingStore, Allocator.Persistent);

            clipperIntersections.Dispose();
            clippers.Dispose();
            childrenChangedList.Dispose();
            queryBuffer.Dispose();
            gridPlaceList.Dispose();
            gridColTemplateBuffer.Dispose();
            gridRowTemplateBuffer.Dispose();
            gridColAutoSizeBuffer.Dispose();
            gridRowAutoSizeBuffer.Dispose();
            gridColTrackBuffer.Dispose();
            clipperBoundsList.Dispose();
            gridRowTrackBuffer.Dispose();
            rootList.Dispose();
            ignoredLayoutList.Dispose();
            specialLayoutList.Dispose();

            for (int i = 0; i < layoutContexts.size; i++) {
                layoutContexts[i].Dispose();
            }

            layoutContexts.Clear();
        }

        private void ResizeBackingStore(int newCapacity) {
            // maybe resize in steps of 512? dunno what a good size is, app dependent
            newCapacity = BitUtil.EnsurePowerOfTwo(newCapacity);
            layoutBackingStore = TypedUnsafe.ResizeSplitBuffer(
                ref layoutResultTable.array,
                ref horizontalLayoutInfoTable.array,
                ref verticalLayoutInfoTable.array,
                ref layoutHierarchyTable.array,
                ref layoutBoxTable.array,
                ref paddingBorderMarginTable.array,
                elementCapacity,
                newCapacity,
                Allocator.Persistent,
                true
            );

            positionBackingStore = TypedUnsafe.ResizeSplitBuffer(
                ref clipInfoTable.array,
                ref transformInfoTable.array,
                ref alignmentInfoHorizontal.array,
                ref alignmentInfoVertical.array,
                ref localMatrices.array,
                ref worldMatrices.array,
                elementCapacity,
                newCapacity,
                Allocator.Persistent,
                true
            );

            *tablePointers = new LayoutDataTables() {
                layoutBoxInfo = layoutResultTable.array,
                localMatrices = localMatrices.array,
                worldMatrices = worldMatrices.array
            };

            elementCapacity = newCapacity;
        }

        private ElementId scrollBoxHead;
        private ElementId scrollBoxTail;

        internal void RegisterScrollBox(ElementId elementId) {
            if (scrollBoxHead == default) {
                scrollBoxHead = elementId;
                scrollBoxTail = elementId;
                layoutBoxTable[elementId].scroll.nextScrollBox = default;
                layoutBoxTable[elementId].scroll.prevScrollBox = default;
            }
            else {
                layoutBoxTable[scrollBoxTail].scroll.nextScrollBox = elementId;
                layoutBoxTable[elementId].scroll.prevScrollBox = scrollBoxTail;
                scrollBoxTail = elementId;
            }
        }

    }

}