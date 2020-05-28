using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;

// ReSharper disable RedundantCaseLabel

namespace UIForia.Systems {

    public struct LayoutBoxId {

        public int instanceId;
        public LayoutType layoutBoxType;

    }
    
    public class LayoutSystem : ILayoutSystem {

        internal Application application;
        private LightList<LayoutRunner> runners;
        public ElementSystem elementSystem;

        private SizedArray<TextLayoutBox> textLayoutPool;
        private SizedArray<FlexLayoutBox> flexLayoutPool;
        private SizedArray<GridLayoutBox> gridLayoutPool;
        private SizedArray<ImageLayoutBox> imageLayoutPool;
        private SizedArray<RootLayoutBox> rootLayoutPool;
        private SizedArray<StackLayoutBox> stackLayoutPool;
        private SizedArray<ScrollViewLayoutBox> scrollLayoutPool;
        private SizedArray<TranscludedLayoutBox> transcludedLayoutPool;

        private DataList<FlexLayoutInfo>.Shared flexLayoutInfo;
        
        public LayoutSystem(Application application, ElementSystem elementSystem) {
            this.application = application;
            this.elementSystem = elementSystem;
            this.runners = new LightList<LayoutRunner>();

            this.textLayoutPool = new SizedArray<TextLayoutBox>(32);
            this.flexLayoutPool = new SizedArray<FlexLayoutBox>(32);
            this.gridLayoutPool = new SizedArray<GridLayoutBox>(16);
            this.imageLayoutPool = new SizedArray<ImageLayoutBox>(16);
            this.rootLayoutPool = new SizedArray<RootLayoutBox>(4);
            this.stackLayoutPool = new SizedArray<StackLayoutBox>(16);
            this.scrollLayoutPool = new SizedArray<ScrollViewLayoutBox>(16);
            this.transcludedLayoutPool = new SizedArray<TranscludedLayoutBox>(16);

            application.onViewsSorted += uiViews => { runners.Sort((a, b) => Array.IndexOf(uiViews, b.rootElement.View) - Array.IndexOf(uiViews, a.rootElement.View)); };
        }
        

        // set box to null when disabling? should be cheap to grab a new one from pool at that point when re-enabling
        // just need to re-initialize styles which I think wont be that expensive
        // also triggered for destroy
        public void HandleElementDisabled(DataList<ElementId>.Shared disabledElements) {

            for (int i = 0; i < disabledElements.size; i++) {

                UIElement element = elementSystem.instanceTable[disabledElements[i].index];

                if (element.layoutBox != null) {
                    // add to pool
                    LayoutBox layoutBox = element.layoutBox;
                    element.layoutBox = null;
                    layoutBox.Destroy();

                    switch (layoutBox) {

                        case FlexLayoutBox flexLayoutBox:
                            flexLayoutPool.Add(flexLayoutBox);
                            break;

                        case GridLayoutBox gridLayoutBox:
                            gridLayoutPool.Add(gridLayoutBox);
                            break;

                        case ImageLayoutBox imageLayoutBox:
                            imageLayoutPool.Add(imageLayoutBox);
                            break;

                        case RootLayoutBox rootLayoutBox:
                            rootLayoutPool.Add(rootLayoutBox);
                            break;

                        case ScrollViewLayoutBox scrollViewLayoutBox:
                            scrollLayoutPool.Add(scrollViewLayoutBox);
                            break;

                        case StackLayoutBox stackLayoutBox:
                            stackLayoutPool.Add(stackLayoutBox);
                            break;

                        case TextLayoutBox textLayoutBox:
                            textLayoutPool.Add(textLayoutBox);
                            break;

                        case TranscludedLayoutBox transcludedLayoutBox:
                            // todo -- I think i can kill this
                            transcludedLayoutPool.Add(transcludedLayoutBox);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();

                    }

                }

                elementSystem.layoutBoxes[element.id.index] = null;

            }

            DataList<ElementId>.Shared roots = new DataList<ElementId>.Shared(32, Allocator.TempJob);

            if (disabledElements.size > 1) {
                new FindHierarchyRootElements() {
                    elements = disabledElements,
                    traversalTable = elementSystem.traversalTable,
                    metaTable = elementSystem.metaTable,
                    roots = roots,
                    mask = UIElementFlags.DisableRoot
                }.Run();
            }
            else {
                roots.Add(disabledElements[0]);
            }

            // no need to burst this, usually only very few elements in this list 
            // cost of job running is probably larger than cost of running this in managed.
            // note: any elements that were ignored will be removed from ignore list in the runner
            // as a pre-process step before running layout. I don't need to search for them here.
            for (int i = 0; i < roots.size; i++) {
                ElementId elementId = roots[i];
                ref LayoutHierarchyInfo layoutInfo = ref elementSystem.layoutHierarchyTable[elementId];
                ref LayoutMetaData metaData = ref elementSystem.layoutMetaDataTable[elementId];

                if (metaData.layoutBehavior == LayoutBehavior.Ignored) {
                    // no-op, no work to do if we were already ignored
                }
                else if (metaData.layoutBehavior == LayoutBehavior.TranscludeChildren) {
                    // todo -- Mark parent for layout here
                    // if was transcluded, need to remove all children from parent and mark that parent for layout. needs to handle multiple transclusion levels
                    throw new NotImplementedException();
                }
                else {

                    // maybe need to worry about the sibling also being disabled
                    if (layoutInfo.prevSiblingId != default && elementSystem.IsEnabled(layoutInfo.prevSiblingId)) {
                        ref LayoutHierarchyInfo prevSiblingInfo = ref elementSystem.layoutHierarchyTable[layoutInfo.prevSiblingId];
                        prevSiblingInfo.nextSiblingId = layoutInfo.nextSiblingId;
                    }

                    if (layoutInfo.nextSiblingId != default && elementSystem.IsEnabled(layoutInfo.nextSiblingId)) {
                        ref LayoutHierarchyInfo nextSiblingInfo = ref elementSystem.layoutHierarchyTable[layoutInfo.nextSiblingId];
                        nextSiblingInfo.prevSiblingId = layoutInfo.prevSiblingId;
                    }

                    if (layoutInfo.parentId != default) {
                        ref LayoutHierarchyInfo parentInfo = ref elementSystem.layoutHierarchyTable[layoutInfo.parentId];
                        parentInfo.childCount--;
                        if (parentInfo.firstChildId == elementId) {
                            parentInfo.firstChildId = layoutInfo.nextSiblingId;
                        }

                        if (parentInfo.lastChildId == elementId) {
                            parentInfo.lastChildId = layoutInfo.prevSiblingId;
                        }
                    }

                    // todo -- Mark parent for layout here

                }

                layoutInfo = default;
            }

        }

        private int flexFreeList;
        private DataList<FlexLayoutBoxBurst> flexLayoutBoxes;
        
        public void InitializeLayoutBox(UIElement element) {

            LayoutType layoutType = element.style.LayoutType;
            
            switch (element) {
                // case UITextElement _:
                //     layoutBox = textLayoutPool.size > 0 ? textLayoutPool.RemoveLast() : new TextLayoutBox();
                //     break;
                //
                // case ScrollView _:
                //     layoutBox = scrollLayoutPool.size > 0 ? scrollLayoutPool.RemoveLast() : new ScrollViewLayoutBox();
                //     break;
                //
                // case UIImageElement _:
                //     layoutBox = imageLayoutPool.size > 0 ? imageLayoutPool.RemoveLast() : new ImageLayoutBox();
                //     break;

                default:
                    switch (layoutType) { // todo -- use meta data
                        default:
                        case LayoutType.Unset:
                        case LayoutType.Flex:
                            // layoutBoxTable[element.id] = new LayoutRunner2.LayoutBoxUnion() {
                            //     flex = 
                            // }
                            break;

                        case LayoutType.Grid:
                            // layoutBox = gridLayoutPool.size > 0 ? gridLayoutPool.RemoveLast() : new GridLayoutBox();
                            break;

                        case LayoutType.Radial:
                            throw new NotImplementedException();

                        case LayoutType.Stack:
                            // layoutBox = stackLayoutPool.size > 0 ? stackLayoutPool.RemoveLast() : new StackLayoutBox();
                            break;
                    }

                    break;
            }

            // elementSystem.layoutBoxes[element.id.index] = layoutBox;
            // element.layoutBox = layoutBox; // todo -- remove
            elementSystem.layoutHierarchyTable[element.id] = default;
            
            
        }
        
        // also triggered for create
        public void HandleElementEnabled(DataList<ElementId>.Shared enabledElements) {
            for (int i = 0; i < enabledElements.size; i++) {

                UIElement element = elementSystem.instanceTable[enabledElements[i].index];

                LayoutBox layoutBox = null;
                switch (element) {
                    case UITextElement _:
                        layoutBox = textLayoutPool.size > 0 ? textLayoutPool.RemoveLast() : new TextLayoutBox();
                        break;

                    case ScrollView _:
                        layoutBox = scrollLayoutPool.size > 0 ? scrollLayoutPool.RemoveLast() : new ScrollViewLayoutBox();
                        break;

                    case UIImageElement _:
                        layoutBox = imageLayoutPool.size > 0 ? imageLayoutPool.RemoveLast() : new ImageLayoutBox();
                        break;

                    default:
                        switch (element.style.LayoutType) { // todo -- use meta data
                            default:
                            case LayoutType.Unset:
                            case LayoutType.Flex:
                                layoutBox = flexLayoutPool.size > 0 ? flexLayoutPool.RemoveLast() : new FlexLayoutBox(1);
                                break;

                            case LayoutType.Grid:
                                layoutBox = gridLayoutPool.size > 0 ? gridLayoutPool.RemoveLast() : new GridLayoutBox();
                                break;

                            case LayoutType.Radial:
                                throw new NotImplementedException();

                            case LayoutType.Stack:
                                layoutBox = stackLayoutPool.size > 0 ? stackLayoutPool.RemoveLast() : new StackLayoutBox();
                                break;
                        }

                        break;
                }

                elementSystem.layoutBoxes[element.id.index] = layoutBox;
                element.layoutBox = layoutBox; // todo -- remove
                elementSystem.layoutHierarchyTable[element.id] = default;
            }

            for (int i = 0; i < enabledElements.size; i++) {
                
                UIElement element = elementSystem.instanceTable[enabledElements[i].index];
                
                element.layoutBox.Initialize(this, elementSystem, element, application.frameId);
                // this point styles are all final for the frame because we ignore changesets for elements enabled this frame
                UIStyleSet style = element.style;

                InitializeLayoutBox(element);
                
                TransformInfo transformInfo = new TransformInfo() {
                    positionX = style.TransformPositionX,
                    positionY = style.TransformPositionY,
                    scaleX = style.TransformScaleX,
                    scaleY = style.TransformScaleY,
                    rotation = style.TransformRotation,
                    pivotX = style.TransformPivotX,
                    pivotY = style.TransformPivotY
                };

                // only create if needed, allocate an id. how do I recycle it? when pooling the box probably
                AlignmentInfo alignmentInfoHorizontal = new AlignmentInfo() {
                    origin = style.AlignmentOriginX,
                    offset = style.AlignmentOffsetX,
                    direction = style.AlignmentDirectionX,
                    target = style.AlignmentTargetX,
                    boundary = style.AlignmentBoundaryX
                };

                // only create if needed, allocate an id
                AlignmentInfo alignmentInfoVertical = new AlignmentInfo() {
                    origin = style.AlignmentOriginY,
                    offset = style.AlignmentOffsetY,
                    direction = style.AlignmentDirectionY,
                    target = style.AlignmentTargetY,
                    boundary = style.AlignmentBoundaryY
                };

                LayoutBehavior behavior = style.LayoutBehavior;
                ref LayoutMetaData meta = ref elementSystem.layoutMetaDataTable[element.id];
                meta.layoutBehavior = behavior;
                meta.horizontalFit = style.FitItemsHorizontal;
                meta.verticalFit = style.FitItemsVertical;
                meta.requiresRebuild = true;
                meta.isWidthContentBased = style.PreferredWidth.IsContentBased;
                meta.isHeightContentBased = style.PreferredHeight.IsContentBased;
                meta.widthBlockProvider = false;
                meta.heightBlockProvider = false;

                
                // is layout root = no fit & fixed size on axis

                // if (TransformNotIdentity) {
                // transformList.Add(transformInfo);
                // }

                // if require horizontal align
                // horizontal align.add()
                // if require vertial align
                // vertical align.add()

            }

            DataList<ElementId>.Shared roots = new DataList<ElementId>.Shared(32, Allocator.TempJob);

            if (enabledElements.size > 1) {
                new FindHierarchyRootElements() {
                    elements = enabledElements,
                    traversalTable = elementSystem.traversalTable,
                    metaTable = elementSystem.metaTable,
                    roots = roots,
                    mask = UIElementFlags.EnabledRoot
                }.Run();
            }
            else {
                roots.Add(enabledElements[0]);
            }

            // distribute these into corresponding layout runners after gathering into this list
            DataList<ElementId>.Shared ignoredLayoutList = new DataList<ElementId>.Shared(32, Allocator.TempJob);

            new HierarchyBuildJob() {
                roots = roots,
                layoutIgnoredList = ignoredLayoutList,
                hierarchyTable = elementSystem.hierarchyTable,
                metaTable = elementSystem.metaTable,
                layoutHierarchyTable = elementSystem.layoutHierarchyTable,
                layoutMetaTable = elementSystem.layoutMetaDataTable
            }.Run();

            for (int i = 0; i < enabledElements.size; i++) {
                LayoutBox layoutBox = elementSystem.layoutBoxes[enabledElements[i].index];
                LayoutHierarchyInfo layoutInfo = elementSystem.layoutHierarchyTable[enabledElements[i]];

                // out of bounds lookups are fine here since 0 is always invalid and will just return null
                layoutBox.firstChild = elementSystem.layoutBoxes[layoutInfo.firstChildId.index];
                layoutBox.parent = elementSystem.layoutBoxes[layoutInfo.parentId.index];
                layoutBox.nextSibling = elementSystem.layoutBoxes[layoutInfo.nextSiblingId.index];
                layoutBox.layoutParentId = layoutInfo.parentId;
                layoutBox.OnChildrenChanged();
            }

            for (int i = 0; i < roots.size; i++) {
                // todo -- tell root parents children changed & mark for layout
                
                LayoutHierarchyInfo layoutInfo = elementSystem.layoutHierarchyTable[roots[i]];
                LayoutHierarchyInfo parentLayoutInfo = elementSystem.layoutHierarchyTable[layoutInfo.parentId];
                LayoutBox layoutBox = elementSystem.layoutBoxes[layoutInfo.parentId.index];
                layoutBox.firstChild = elementSystem.layoutBoxes[parentLayoutInfo.firstChildId.index];

                layoutBox.OnChildrenChanged();

            }

            ignoredLayoutList.Dispose();
            roots.Dispose();
        }

        /// <summary>
        /// This job takes all the elements that were enabled or created this frame and finds
        /// the highest (lowest depth) element for all hierarchies.
        /// </summary>
        [BurstCompile]
        private unsafe struct FindHierarchyRootElements : IJob {

            public DataList<ElementId>.Shared elements;
            public ElementTable<ElementTraversalInfo> traversalTable;
            public ElementTable<ElementMetaInfo> metaTable;
            public DataList<ElementId>.Shared roots;
            public UIElementFlags mask;

            public void Execute() {

                for (int i = 0; i < elements.size; i++) {

                    if ((metaTable[elements[i]].flags & mask) != 0) {
                        roots.Add(elements[i]);
                    }

                }

                ElementId* buffer = stackalloc ElementId[roots.size];

                int bufferSize = 0;

                buffer[bufferSize++] = roots[0];

                for (int i = 1; i < roots.size; i++) {

                    ref ElementTraversalInfo element = ref traversalTable[roots[i]];

                    bool add = true;
                    for (int j = 0; j < bufferSize; j++) {
                        ElementId target = buffer[j];
                        // if what is in the buffer is a descendent of 'this', replace the thing in the buffer
                        if (traversalTable[target].IsDescendentOf(element)) {
                            element = ref traversalTable[target];
                            buffer[j] = target;
                            add = false;
                        }
                    }

                    if (add) {
                        buffer[bufferSize++] = roots[i];
                    }
                }

            }

        }

        // only called for elements that were not enabled this frame
        public void HandleStylePropertyUpdates(UIElement element, StyleProperty[] properties, int propertyCount) {
            // bool checkAlignHorizontal = false;
            // bool updateAlignVertical = false;
            // bool updateTransform = false;
            //
            // ElementTable<ElementMetaInfo> metaTable = elementSystem.metaTable;
            // ref ElementMetaInfo metaInfo = ref metaTable[element.id];
            //
            // for (int i = 0; i < propertyCount; i++) {
            //     ref StyleProperty property = ref properties[i];
            //     switch (property.propertyId) {
            //         case StylePropertyId.ClipBehavior:
            //
            //             element.layoutBox.flags |= LayoutBoxFlags.RecomputeClipping;
            //             //element.layoutBox.clipBehavior = property.AsClipBehavior;
            //
            //             break;
            //
            //         case StylePropertyId.ClipBounds:
            //             //element.layoutBox.clipBounds = property.AsClipBounds;
            //
            //             break;
            //
            //         case StylePropertyId.OverflowX:
            //         case StylePropertyId.OverflowY:
            //             element.layoutBox.UpdateClipper();
            //             break;
            //
            //         case StylePropertyId.LayoutType:
            //             ChangeLayoutBox(element, property.AsLayoutType);
            //             break;
            //
            //         case StylePropertyId.LayoutBehavior:
            //             metaInfo.flags |= UIElementFlags.LayoutTypeOrBehaviorDirty;
            //             break;
            //
            //         case StylePropertyId.TransformRotation: {
            //             //element.layoutBox.transformRotation = property.AsFloat;
            //
            //             updateTransform = true;
            //             break;
            //         }
            //
            //         case StylePropertyId.TransformPositionX: {
            //             //element.layoutBox.transformPositionX = property.AsOffsetMeasurement;
            //
            //             updateTransform = true;
            //             break;
            //         }
            //
            //         case StylePropertyId.TransformPositionY: {
            //             //element.layoutBox.transformPositionY = property.AsOffsetMeasurement;
            //
            //             updateTransform = true;
            //             break;
            //         }
            //
            //         case StylePropertyId.TransformScaleX: {
            //             //element.layoutBox.transformScaleX = property.AsFloat;
            //
            //             updateTransform = true;
            //             break;
            //         }
            //
            //         case StylePropertyId.TransformScaleY: {
            //             //element.layoutBox.transformScaleY = property.AsFloat;
            //
            //             updateTransform = true;
            //             break;
            //         }
            //
            //         case StylePropertyId.TransformPivotX: {
            //             //element.layoutBox.transformPivotX = property.AsUIFixedLength;
            //
            //             updateTransform = true;
            //             break;
            //         }
            //
            //         case StylePropertyId.TransformPivotY:
            //             //element.layoutBox.transformPivotY = property.AsUIFixedLength;
            //
            //             updateTransform = true;
            //             break;
            //
            //         case StylePropertyId.AlignmentTargetX:
            //         case StylePropertyId.AlignmentOriginX:
            //         case StylePropertyId.AlignmentOffsetX:
            //         case StylePropertyId.AlignmentDirectionX:
            //         case StylePropertyId.AlignmentBoundaryX:
            //             checkAlignHorizontal = true;
            //             break;
            //
            //         case StylePropertyId.AlignmentTargetY:
            //         case StylePropertyId.AlignmentOriginY:
            //         case StylePropertyId.AlignmentOffsetY:
            //         case StylePropertyId.AlignmentDirectionY:
            //         case StylePropertyId.AlignmentBoundaryY:
            //             updateAlignVertical = true;
            //             break;
            //
            //         case StylePropertyId.MinWidth:
            //         case StylePropertyId.MaxWidth:
            //         case StylePropertyId.PreferredWidth:
            //             element.layoutBox.UpdateBlockProviderWidth();
            //             element.layoutBox.MarkForLayoutHorizontal();
            //             break;
            //
            //         case StylePropertyId.MinHeight:
            //         case StylePropertyId.MaxHeight:
            //         case StylePropertyId.PreferredHeight:
            //             element.layoutBox.UpdateBlockProviderHeight();
            //             element.layoutBox.MarkForLayoutVertical();
            //             break;
            //
            //         case StylePropertyId.PaddingLeft:
            //         case StylePropertyId.PaddingRight:
            //         case StylePropertyId.BorderLeft:
            //         case StylePropertyId.BorderRight:
            //             element.layoutBox.flags |= LayoutBoxFlags.ContentAreaWidthChanged;
            //             break;
            //
            //         case StylePropertyId.PaddingTop:
            //         case StylePropertyId.PaddingBottom:
            //         case StylePropertyId.BorderTop:
            //         case StylePropertyId.BorderBottom:
            //             if (element.layoutBox != null) {
            //                 element.layoutBox.flags |= LayoutBoxFlags.ContentAreaHeightChanged;
            //             }
            //
            //             break;
            //
            //         case StylePropertyId.LayoutFitHorizontal:
            //             // metaInfo.flags |= UIElementFlags.LayoutFitWidthDirty;
            //             break;
            //
            //         case StylePropertyId.LayoutFitVertical:
            //             //metaInfo.flags |= UIElementFlags.LayoutFitHeightDirty;
            //             break;
            //
            //         case StylePropertyId.ZIndex:
            //             //element.layoutBox.zIndex = property.AsInt;
            //             break;
            //     }
            // }
            //
            // if (updateTransform) {
            //     float rotation = element.style.TransformRotation;
            //     float scaleX = element.style.TransformScaleX;
            //     float scaleY = element.style.TransformScaleY;
            //     float positionX = element.style.TransformPositionX.value;
            //     float positionY = element.style.TransformPositionY.value;
            //
            //     if (rotation != 0 || scaleX != 1 || scaleY != 1 || positionX != 0 || positionY != 0) {
            //         metaInfo.flags |= UIElementFlags.LayoutTransformNotIdentity;
            //     }
            //     else {
            //         metaInfo.flags &= ~UIElementFlags.LayoutTransformNotIdentity;
            //     }
            //
            //     element.layoutBox.flags |= LayoutBoxFlags.TransformDirty;
            // }
            //
            // LayoutBox layoutBox = element.layoutBox;
            // if (checkAlignHorizontal) {
            //     layoutBox.UpdateRequiresHorizontalAlignment();
            // }
            //
            // if (updateAlignVertical) {
            //     layoutBox.UpdateRequiresVerticalAlignment();
            // }
            //
            // layoutBox.OnStyleChanged(properties, propertyCount);
            // // don't need to null check since root box will never have a style changed
            // layoutBox.OnChildStyleChanged(element.layoutBox, properties, propertyCount);
        }

        public void RunLayout() {
            for (int i = 0; i < runners.size; i++) {
                runners[i].RunLayout();
            }
        }

        public void OnViewAdded(UIView view) {
            runners.Add(new LayoutRunner(this, view, view.dummyRoot));
        }

        // todo -- re-instate this
        public void OnViewRemoved(UIView view) {
            for (int i = 0; i < runners.size; i++) {
                if (runners[i].rootElement == view.dummyRoot) {
                    runners.RemoveAt(i);
                    return;
                }
            }
        }

        public IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn) {
            for (int i = 0; i < runners.size; i++) {
                runners[i].QueryPoint(point, retn);
            }

            return retn;
        }

        public LayoutRunner GetLayoutRunner(UIElement viewRoot) {
            for (int i = 0; i < runners.size; i++) {
                if (runners.array[i].rootElement == viewRoot) {
                    return runners.array[i];
                }
            }

            return null;
        }

    }

}