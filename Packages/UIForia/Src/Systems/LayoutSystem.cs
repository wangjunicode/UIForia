using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Systems {

    public class LayoutSystem : ILayoutSystem {

        public const int TextLayoutPoolKey = 100;
        public const int ImageLayoutPoolKey = 200;

        protected readonly IStyleSystem m_StyleSystem;
        protected readonly IntMap<LayoutBox> m_LayoutBoxMap;
        protected readonly LightList<TextLayoutBox> m_TextLayoutBoxes;

        private Size m_ScreenSize;
        private readonly LightList<LayoutBox> m_VisibleBoxList;

        private static readonly IComparer<LayoutBox> comparer = new DepthComparer();
        private readonly Dictionary<int, LayoutBoxPool> layoutBoxPoolMap;
        private readonly LightList<LayoutBox> toLayout = new LightList<LayoutBox>(128);
        private readonly Application application;

        public LayoutSystem(Application application, IStyleSystem styleSystem) {
            this.application = application;
            this.m_StyleSystem = styleSystem;
            this.m_LayoutBoxMap = new IntMap<LayoutBox>();
            this.m_VisibleBoxList = new LightList<LayoutBox>();
            this.m_TextLayoutBoxes = new LightList<TextLayoutBox>(64);
            this.m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
            this.layoutBoxPoolMap = new Dictionary<int, LayoutBoxPool>();

            this.layoutBoxPoolMap[(int) LayoutType.Flex] = new LayoutBoxPool<FlexLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Grid] = new LayoutBoxPool<GridLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Radial] = new LayoutBoxPool<RadialLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Fixed] = new LayoutBoxPool<FixedLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Flow] = new LayoutBoxPool<FlowLayoutBox>();
            this.layoutBoxPoolMap[TextLayoutPoolKey] = new LayoutBoxPool<TextLayoutBox>();
            this.layoutBoxPoolMap[ImageLayoutPoolKey] = new LayoutBoxPool<ImageLayoutBox>();
        }

        public void OnReset() {
            m_LayoutBoxMap.Clear();
            m_VisibleBoxList.Clear();
        }

        public void OnUpdate() {
            // todo -- should this be a list per-view?
            m_VisibleBoxList.Clear();


            TextLayoutBox[] textLayouts = m_TextLayoutBoxes.Array;
            for (int i = 0; i < m_TextLayoutBoxes.Count; i++) {
                if (textLayouts[i].TextInfo.LayoutDirty) {
                    textLayouts[i].RequestContentSizeChangeLayout();
                }
            }

            for (int i = 0; i < application.m_Views.Count; i++) {
                RunLayout2(application.m_Views[i]);
            }
        }

        /*
         * for each view
         *     gather list of elements to consider for layout
         *     
         *     if element children exceed width bounds
         *     if overflow y is scroll & scroll layout is push content
         *     run scrollbar vertical layout
         *     run layout again w/ width - scroll bar width
         *     set clip rect x - width to 0 -> scroll bar x (or invert if scroll position inverted)
         *
         *     traverse to find things to layout
         *     run those layouts
         *     compute matrices
         *     compute clip rects
         *     update query grid
         *     copy properites to layout result
         */

        private void CollectLayoutBoxes(UIView view) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            UIElement rootElement = view.rootElement;

            for (int i = 0; i < rootElement.children.Count; i++) {
                stack.Push(rootElement.children[i]);
            }

            int idx = 0;

            toLayout.QuickClear();
            int elementCount = view.GetElementCount();
            toLayout.EnsureCapacity(elementCount);
            LayoutBox[] toLayoutArray = toLayout.Array;

            int viewDepth = view.depth;

            while (stack.Count > 0) {
                UIElement currentElement = stack.PopUnchecked();

                if (currentElement.isDisabled) {
                    continue;
                }

                LayoutBox currentBox = m_LayoutBoxMap.GetOrDefault(currentElement.id);
                currentBox.traversalIndex = idx;
                currentBox.viewDepthIdx = viewDepth;

                toLayoutArray[idx++] = currentBox;

                UIElement[] childArray = currentElement.children.Array;
                int childCount = currentElement.children.Count;
                for (int i = childCount - 1; i >= 0; i--) {
                    stack.Push(childArray[i]);
                }
            }

            toLayout.Count = idx;
            LightStack<UIElement>.Release(ref stack);
        }

        // todo -- build quad tree for queries
        private unsafe struct QuadTreeNode {

            public fixed int ids[12]; // index into visibility list
            public fixed int children[4];
            public int x;
            public int y;
            public int width;
            public int height;

        }

        public void RunLayout2(UIView view) {
            m_VisibleBoxList.QuickClear();
            view.visibleElements.QuickClear();

            UIElement rootElement = view.rootElement;

            LayoutBox rootBox = m_LayoutBoxMap.GetOrDefault(rootElement.id);
            rootBox.element.layoutResult.matrix = SVGXMatrix.identity;
            rootBox.prefWidth = new UIMeasurement(1, UIMeasurementUnit.ViewportWidth);
            rootBox.prefHeight = new UIMeasurement(1, UIMeasurementUnit.ViewportHeight);

            rootBox.clipRect = new Rect(0, 0, Screen.width, Screen.height);

            rootBox.allocatedWidth = view.Viewport.width;
            rootBox.allocatedHeight = view.Viewport.height;
            rootBox.actualWidth = rootBox.allocatedWidth;
            rootBox.actualHeight = rootBox.allocatedHeight;

            // todo -- only if changed
            if (view.sizeChanged) {
                rootBox.RunLayout();
                view.sizeChanged = false; // todo - dont do this here
            }

            CollectLayoutBoxes(view);

            LightList<LayoutBox> leaves = new LightList<LayoutBox>();

            LayoutBox[] toLayoutArray = toLayout.Array;
            int toLayoutCount = toLayout.Count;

            for (int i = 0; i < toLayoutCount; i++) {
                LayoutBox box = toLayoutArray[i];

                if (box.IsIgnored) {
                    float currentWidth = box.allocatedWidth;
                    float currentHeight = box.allocatedHeight;
                    box.allocatedWidth = box.GetWidths().clampedSize;
                    box.allocatedHeight = box.GetHeights(box.allocatedWidth).clampedSize;
                    box.localX = 0;
                    box.localY = 0;
                    if (box.allocatedWidth != currentWidth || box.allocatedHeight != currentHeight) {
                        box.markedForLayout = true;
                    }
                }

                if (box.markedForLayout) {
                    box.RunLayout();
                    box.markedForLayout = false;
                }

                if (box.children.Count == 0) {
                    leaves.Add(box);
                }
            }

            int leafCount = leaves.Count;
            LayoutBox[] leafArray = leaves.Array;

            for (int i = 0; i < leafCount; i++) {
                LayoutBox current = leafArray[i];
                LayoutBox ptr = current.parent;

                current.xMax = current.localX + current.actualWidth;
                current.yMax = current.localY + current.actualHeight;

                // each element needs a clip rect
                // how do I find it?

                while (ptr != null) {
                    ptr.xMax = math.max(ptr.xMax, ptr.localX + current.xMax);
                    ptr.yMax = math.max(ptr.yMax, ptr.localY + current.yMax);
                    current = ptr;
                    ptr = ptr.parent;
                }
            }

            for (int i = 0; i < toLayoutCount; i++) {
                LayoutBox box = toLayoutArray[i];

                Vector2 scrollOffset = new Vector2();

                LayoutBox parentBox = box.parent;

                if (!box.IsIgnored) {
                    scrollOffset.x = (parentBox.xMax - parentBox.allocatedWidth) * parentBox.element.scrollOffset.x;
                    scrollOffset.y = (parentBox.yMax - parentBox.allocatedHeight) * parentBox.element.scrollOffset.y;
                }

                // clip rect is nearest ancestor w/ overflow handling on a given axis
                // can pass down top to bottom so that parents clip their children according to their parent's clip bounds
                LayoutResult layoutResult = box.element.layoutResult;

                box.clipRect = parentBox.clipRect;
                layoutResult.clipRect = parentBox.clipRect;


                Vector2 localPosition = ResolveLocalPosition(box) - scrollOffset;
                Vector2 localScale = new Vector2(box.transformScaleX, box.transformScaleY);


                Vector2 pivot = box.Pivot;
                SVGXMatrix m;

                if (box.transformRotation != 0) {
                    m = SVGXMatrix.TRS(localPosition, layoutResult.rotation, layoutResult.scale);
                }
                else {
                    m = SVGXMatrix.TranslateScale(localPosition.x, localPosition.y, localScale.x, localScale.y);
                }

                SVGXMatrix parentMatrix = box.parent.element.layoutResult.matrix;

                if (pivot.x != 0 || pivot.y != 0) {
                    SVGXMatrix pivotMat = SVGXMatrix.Translation(new Vector2(box.allocatedWidth * pivot.x, box.allocatedHeight * pivot.y));
                    m = pivotMat * m * pivotMat.Inverse();
                }

                m = parentMatrix * m;
                layoutResult.matrix = m;

                layoutResult.overflowSize = new Size(box.xMax, box.yMax);
                layoutResult.localPosition = localPosition;
                layoutResult.ContentRect = box.ContentRect;

                layoutResult.actualSize = new Size(box.actualWidth, box.actualHeight);
                layoutResult.allocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);

                layoutResult.screenPosition = m.position;

                layoutResult.scale.x = localScale.x;
                layoutResult.scale.y = localScale.y;
                layoutResult.rotation = box.transformRotation;
                layoutResult.pivot = pivot;

                layoutResult.borderRadius = new ResolvedBorderRadius(box.BorderRadiusTopLeft, box.BorderRadiusTopRight, box.BorderRadiusBottomRight, box.BorderRadiusBottomLeft);
                layoutResult.border = new OffsetRect(box.BorderTop, box.BorderRight, box.BorderBottom, box.BorderLeft);
                layoutResult.padding = new OffsetRect(box.PaddingTop, box.PaddingRight, box.PaddingBottom, box.PaddingLeft);

                if (box.style.OverflowX != Overflow.Visible) {
                    // use own value for children
                    box.clipRect.x = m.position.x;
                    box.clipRect.width = box.allocatedWidth;
                }

                if (box.style.OverflowY != Overflow.Visible) {
                    box.clipRect.y = m.position.y;
                    box.clipRect.height = box.allocatedHeight;
                }
            }

            for (int i = 0; i < toLayoutCount; i++) {
                // if height or width is zero
                // if parent overflow is hidden & parent clip bounds ! contains or intersects children
                // if parent overflow is h
                m_VisibleBoxList.Add(toLayoutArray[i]);
            }

            m_VisibleBoxList.Sort(comparer);

            LayoutBox[] boxes = m_VisibleBoxList.Array;

            for (int i = 0; i < m_VisibleBoxList.Count; i++) {
                boxes[i].element.layoutResult.zIndex = i + 1;
                view.visibleElements.Add(boxes[i].element);
            }
        }

        private static Vector2 ResolveLocalPosition(LayoutBox box) {
            Vector2 localPosition = Vector2.zero;

            LayoutBehavior layoutBehavior = box.style.LayoutBehavior;
            TransformBehavior transformBehaviorX = box.transformBehaviorX;
            TransformBehavior transformBehaviorY = box.transformBehaviorY;

            switch (layoutBehavior) {
                case LayoutBehavior.TranscludeChildren:
                    localPosition = new Vector2(0, 0);
                    break;

                case LayoutBehavior.Ignored:
                case LayoutBehavior.Normal:

                    switch (transformBehaviorX) {
                        case TransformBehavior.AnchorMinOffset:
                            localPosition.x = box.AnchorLeft + box.TransformX;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.x = box.AnchorRight - box.TransformX - box.actualWidth;
                            break;
                        case TransformBehavior.LayoutOffset:
                            localPosition.x = box.localX + box.TransformX;
                            break;
                        default:
                            localPosition.x = box.localX;
                            break;
                    }

                    switch (transformBehaviorY) {
                        case TransformBehavior.AnchorMinOffset:
                            localPosition.y = box.AnchorTop + box.TransformY;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.y = box.AnchorBottom - box.TransformY - box.actualHeight;
                            break;
                        case TransformBehavior.LayoutOffset:
                            localPosition.y = box.localY + box.TransformY;
                            break;
                        default:
                            localPosition.y = box.localY;
                            break;
                    }

                    break;
            }

            return localPosition;
        }

        private void CreateOrDestroyScrollbars(LayoutBox box) {
            UIElement element = box.element;

            if (box.actualHeight <= box.allocatedHeight) {
                LayoutResult lr = box.element.layoutResult;
                lr.scrollbarVerticalSize = Size.Unset;
            }
            else {
                Overflow verticalOverflow = box.style.OverflowY;
                if (verticalOverflow == Overflow.Scroll || verticalOverflow == Overflow.ScrollAndAutoHide) {
                    Scrollbar vertical = Application.GetCustomScrollbar(null);

                    Extents childExtents = GetLocalExtents(box.children);
                    float offsetY = (childExtents.min.y < 0) ? -childExtents.min.y / box.allocatedHeight : 0f;
                    element.scrollOffset = new Vector2(element.scrollOffset.x, offsetY);

                    Size originalAllocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
                    element.layoutResult.actualSize = new Size(box.actualWidth, box.actualHeight);
                    element.layoutResult.allocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);

                    Size verticalScrollbarSize = vertical.RunLayout(element);

                    // this is the push-content case
                    box.allocatedWidth -= (Mathf.Clamp(verticalScrollbarSize.width, 0, box.allocatedWidth));

                    box.RunLayout();

                    element.layoutResult.allocatedSize = originalAllocatedSize;
                    element.layoutResult.scrollbarVerticalSize = verticalScrollbarSize;
                }
            }

//            if (box.actualWidth <= box.allocatedWidth) {
//                if (horizontal != null) {
//                    onDestroyVirtualScrollbar?.Invoke(horizontal);
//                    m_Elements.Remove(horizontal);
//                    m_VirtualElements.Remove(horizontal);
//                    box.horizontalScrollbar = null; // todo -- pool
//                }
//            }
//            else {
//                Overflow horizontalOverflow = box.style.OverflowX;
//                if (horizontal == null && horizontalOverflow == Overflow.Scroll || horizontalOverflow == Overflow.ScrollAndAutoHide) {
//                    horizontal = new VirtualScrollbar(element, ScrollbarOrientation.Horizontal);
//                    // todo -- depth index needs to be set
//
//                    m_Elements.Add(horizontal);
//
//                    m_VirtualElements.Add(horizontal);
//
//                    Extents childExtents = GetLocalExtents(box.children);
//                    float offsetX = (childExtents.min.x < 0) ? -childExtents.min.x / box.allocatedWidth : 0f;
//                    element.scrollOffset = new Vector2(element.scrollOffset.y, offsetX);
//
//                    onCreateVirtualScrollbar?.Invoke(horizontal);
//                    box.horizontalScrollbar = horizontal;
//                }
//            }
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            CreateLayoutBox(view.rootElement);
        }

        public void OnViewRemoved(UIView view) {
            m_LayoutBoxMap.GetOrDefault(view.rootElement.id)?.Release();
        }

        private void HandleStylePropertyChanged(UIElement element, StructList<StyleProperty> properties) {
            // todo early-out if we haven't had a layout pass for the element yet
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) {
                return;
            }

            bool notifyParent = box.parent != null && (box.style.LayoutBehavior & LayoutBehavior.Ignored) == 0 && box.element.isEnabled;
            bool invalidatePreferredSizeCache = false;
            bool layoutTypeChanged = false;

            for (int i = 0; i < properties.Count; i++) {
                StyleProperty property = properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.PaddingLeft:
                        box.paddingLeft = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.PaddingRight:
                        box.paddingRight = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.PaddingTop:
                        box.paddingTop = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.PaddingBottom:
                        box.paddingBottom = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderLeft:
                        box.borderLeft = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRight:
                        box.borderRight = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderTop:
                        box.borderTop = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderBottom:
                        box.borderBottom = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRadiusTopLeft:
                        box.borderRadiusTopLeft = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRadiusTopRight:
                        box.borderRadiusTopRight = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRadiusBottomLeft:
                        box.borderRadiusBottomLeft = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRadiusBottomRight:
                        box.borderRadiusBottomRight = property.AsUIFixedLength;
                        break;

                    // todo -- margin should be a fixed measurement probably
                    case StylePropertyId.MarginLeft:
                        box.marginLeft = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MarginRight:
                        box.marginRight = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MarginTop:
                        box.marginTop = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MarginBottom:
                        box.marginBottom = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.TransformPivotX:
                        box.transformPivotX = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.TransformPivotY:
                        box.transformPivotY = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.TransformPositionX:
                        box.transformPositionX = property.AsTransformOffset;
                        break;

                    case StylePropertyId.TransformPositionY:
                        box.transformPositionY = property.AsTransformOffset;
                        break;

                    case StylePropertyId.TransformBehaviorX:
                        box.transformBehaviorX = property.AsTransformBehavior;
                        break;

                    case StylePropertyId.TransformBehaviorY:
                        box.transformBehaviorY = property.AsTransformBehavior;
                        break;

                    case StylePropertyId.TransformRotation:
                        box.transformRotation = property.AsFloat;
                        break;

                    case StylePropertyId.TransformScaleX:
                        box.transformScaleX = property.AsFloat;
                        break;

                    case StylePropertyId.TransformScaleY:
                        box.transformScaleY = property.AsFloat;
                        break;

                    case StylePropertyId.PreferredWidth:
                        box.prefWidth = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.PreferredHeight:
                        box.prefHeight = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MinWidth:
                        box.minWidth = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MinHeight:
                        box.minHeight = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MaxWidth:
                        box.maxWidth = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MaxHeight:
                        box.maxHeight = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.ZIndex:
                        box.zIndex = property.AsInt;
                        break;

                    case StylePropertyId.Layer:
                        box.layer = property.AsInt;
                        break;

                    case StylePropertyId.LayoutBehavior:
                        box.layoutBehavior = property.AsLayoutBehavior;
                        box.UpdateChildren();

                        break;
                    case StylePropertyId.LayoutType:
                        layoutTypeChanged = true;
                        break;
                }

                if (!invalidatePreferredSizeCache) {
                    switch (property.propertyId) {
                        case StylePropertyId.MinWidth:
                        case StylePropertyId.MaxWidth:
                        case StylePropertyId.PreferredWidth:
                            invalidatePreferredSizeCache = true;
                            break;
                        case StylePropertyId.MinHeight:
                        case StylePropertyId.MaxHeight:
                        case StylePropertyId.PreferredHeight:
                            invalidatePreferredSizeCache = true;
                            break;
                        case StylePropertyId.AnchorTop:
                        case StylePropertyId.AnchorRight:
                        case StylePropertyId.AnchorBottom:
                        case StylePropertyId.AnchorLeft:
                        case StylePropertyId.AnchorTarget:
                            invalidatePreferredSizeCache = true;
                            break;
                    }
                }
            }

            if (layoutTypeChanged) {
                HandleLayoutChanged(element);
                box.parent?.OnChildStylePropertyChanged(box, properties);
            }
            else {
                if (invalidatePreferredSizeCache) {
                    if (notifyParent) {
                        box.RequestContentSizeChangeLayout();
                    }

                    box.InvalidatePreferredSizeCache();
                }

                box.OnStylePropertyChanged(properties);

                if (notifyParent) {
                    box.parent.OnChildStylePropertyChanged(box, properties);
                }
            }
        }

        private void HandleLayoutChanged(UIElement element) {
            LayoutBox box;
            if (!m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                CreateLayoutBox(element);
                return;
            }

            LayoutBox parent = box.parent;
            LayoutBox replace = CreateLayoutBox(element);

            replace.allocatedWidth = box.allocatedWidth;
            replace.allocatedHeight = box.allocatedHeight;

            replace.parent = parent;
            UpdateChildren(replace);
            UpdateChildren(parent);
            box.Release();
        }

        public struct LayoutBoxPair {

            public UIElement element;
            public LayoutBox parentBox;

            public LayoutBoxPair(UIElement element, LayoutBox parentBox) {
                this.element = element;
                this.parentBox = parentBox;
            }

        }

        public void OnElementEnabled(UIElement element) {
            // none of these boxes should exist for the whole hierarchy, create them

            LightList<LayoutBox> toUpdateList = LightListPool<LayoutBox>.Get();
            LightStack<LayoutBoxPair> stack = LightStack<LayoutBoxPair>.Get();

            if (element.parent != null) {
                stack.Push(new LayoutBoxPair(element, m_LayoutBoxMap.GetOrDefault(element.parent.id)));
            }
            else {
                stack.Push(new LayoutBoxPair(element, null));
            }

            while (stack.Count > 0) {
                LayoutBoxPair current = stack.PopUnchecked();

                if (current.element.isDestroyed || current.element.isDisabled) {
                    continue;
                }

                LayoutBox box = CreateLayoutBox(current.element);
                box.parent = current.parentBox;
                toUpdateList.Add(box);

                int childCount = current.element.children.Count;
                UIElement[] children = current.element.children.Array;

                for (int i = 0; i < childCount; i++) {
                    stack.Push(new LayoutBoxPair(children[i], box));
                }
            }


            int count = toUpdateList.Count;
            LayoutBox[] toUpdate = toUpdateList.Array;

            for (int i = 0; i < count; i++) {
                UpdateChildren(toUpdate[i]);
            }

            if (element.parent != null) {
                LayoutBox ptr = toUpdate[0].parent;
                while (ptr != null) {
                    if (ptr.style.LayoutBehavior != LayoutBehavior.TranscludeChildren) {
                        UpdateChildren(ptr);
                        break;
                    }

                    ptr = ptr.parent;
                }
            }

            LightListPool<LayoutBox>.Release(ref toUpdateList);
            LightStack<LayoutBoxPair>.Release(ref stack);
        }

        public void OnElementDisabled(UIElement element) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            stack.Push(element);
            LayoutBox currentBox = m_LayoutBoxMap.GetOrDefault(element.id);
            LayoutBox parentBox = null;
            if (currentBox != null) {
                parentBox = currentBox.parent;
            }

            while (stack.Count > 0) {
                UIElement current = stack.PopUnchecked();

                if (m_LayoutBoxMap.Remove(current.id, out LayoutBox box)) {
                    box.Release();
                }

                int childCount = current.children.Count;
                UIElement[] children = current.children.Array;
                for (int i = 0; i < childCount; i++) {
                    stack.Push(children[i]);
                }
            }

            LightStack<UIElement>.Release(ref stack);
            if (parentBox != null) {
                UpdateChildren(parentBox);
                parentBox.UpdateChildren();
            }
        }

        public void OnElementDestroyed(UIElement element) {
            if (m_LayoutBoxMap.Remove(element.id, out LayoutBox box)) {
                if (box.parent != null) {
                    UpdateChildren(box.parent);
                }

                m_VisibleBoxList.Remove(box);
                box.Release();
            }
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        private LayoutBox CreateLayoutBox(UIElement element) {
            LayoutBox retn = null;
            if ((element is UITextElement)) {
                TextLayoutBox textLayout = (TextLayoutBox) layoutBoxPoolMap[TextLayoutPoolKey].Get(element);
                m_TextLayoutBoxes.Add(textLayout);
                retn = textLayout;
            }

            else if ((element is UIImageElement)) {
                retn = layoutBoxPoolMap[ImageLayoutPoolKey].Get(element);
            }

            else {
                switch (element.style.LayoutType) {
                    case LayoutType.Flex:
                        retn = layoutBoxPoolMap[(int) LayoutType.Flex].Get(element);
                        break;

                    case LayoutType.Flow:
                        retn = layoutBoxPoolMap[(int) LayoutType.Flow].Get(element);
                        break;

                    case LayoutType.Fixed:
                        retn = layoutBoxPoolMap[(int) LayoutType.Fixed].Get(element);
                        break;

                    case LayoutType.Grid:
                        retn = layoutBoxPoolMap[(int) LayoutType.Grid].Get(element);
                        break;

                    case LayoutType.Radial:
                        retn = layoutBoxPoolMap[(int) LayoutType.Radial].Get(element);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            retn.UpdateFromStyle();
            m_LayoutBoxMap[element.id] = retn;
            return retn;
        }

        public void OnElementCreated(UIElement element) { }

        private void GetChildBoxes(LayoutBox box, LightList<LayoutBox> list) {
            UIElement element = box.element;
            UIElement[] children = element.children.Array;
            int count = element.children.Count;

            for (int i = 0; i < count; i++) {
                LayoutBox childBox = m_LayoutBoxMap[children[i].id];
                if (childBox == null) {
                    continue;
                }

                LayoutBehavior behavior = childBox.style.LayoutBehavior;
                switch (behavior) {
                    case LayoutBehavior.Unset:
                    case LayoutBehavior.Normal:
                        list.Add(childBox);
                        break;
                    case LayoutBehavior.Ignored:
                        break;
                    case LayoutBehavior.TranscludeChildren:
                        GetChildBoxes(childBox, list);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void UpdateChildren(LayoutBox box) {
            UIElement element = box.element;

            if (box.style.LayoutBehavior == LayoutBehavior.TranscludeChildren) {
                return;
            }

            if (element.children == null || element.children.Count == 0) {
                return;
            }

            LightList<LayoutBox> boxes = LightListPool<LayoutBox>.Get();
            boxes.EnsureCapacity(element.children.Count);

            box.children.Clear();
            GetChildBoxes(box, boxes);
            box.children.AddRange(boxes);
            for (int i = 0; i < boxes.Count; i++) {
                boxes[i].parent = box;
            }

            LightListPool<LayoutBox>.Release(ref boxes);
            box.UpdateChildren();
        }

        public List<UIElement> QueryPoint(Vector2 point, List<UIElement> retn) {
            // todo convert to quad tree
            if (retn == null) {
                retn = ListPool<UIElement>.Get();
            }

            for (int i = 0; i < application.m_Views.Count; i++) {
                QueryPointInView(point, application.m_Views[i], retn);
            }

            return retn;
        }

        private static void QueryPointInView(Vector2 point, UIView view, List<UIElement> retn) {
            UIElement[] elements = view.visibleElements.Array;
            int elementCount = view.visibleElements.Count;
            for (int i = 0; i < elementCount; i++) {
                UIElement element = elements[i];
                LayoutResult layoutResult = element.layoutResult;

                // todo make this better
                if (element.isDisabled) {
                    continue;
                }

                if (element is IPointerQueryHandler handler) {
                    if (!handler.ContainsPoint(point)) {
                        continue;
                    }
                }
                else if (!layoutResult.ScreenRect.ContainOrOverlap(point)) {
                    continue;
                }

                UIElement ptr = element.parent;
                while (ptr != null) {
                    Vector2 screenPosition = ptr.layoutResult.screenPosition;
                    if (ptr.style.OverflowX != Overflow.Visible) {
                        if (point.x < screenPosition.x || point.x > screenPosition.x + ptr.layoutResult.actualSize.width) {
                            break;
                        }
                    }

                    if (ptr.style.OverflowY != Overflow.Visible) {
                        if (point.y < screenPosition.y || point.y > screenPosition.y + ptr.layoutResult.actualSize.height) {
                            break;
                        }
                    }

                    ptr = ptr.parent;
                }

                if (ptr == null) {
                    retn.Add(element);
                }
            }
        }

        public OffsetRect GetPaddingRect(UIElement element) {
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) return new OffsetRect();
            return new OffsetRect(
                box.PaddingTop,
                box.PaddingRight,
                box.PaddingBottom,
                box.PaddingLeft
            );
        }

        public OffsetRect GetMarginRect(UIElement element) {
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) return new OffsetRect();
            return new OffsetRect(
                box.GetMarginTop(box.actualWidth),
                box.GetMarginRight(),
                box.GetMarginBottom(box.actualWidth),
                box.GetMarginLeft()
            );
        }

        public OffsetRect GetBorderRect(UIElement element) {
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) return new OffsetRect();
            return new OffsetRect(
                box.BorderTop,
                box.BorderRight,
                box.BorderBottom,
                box.BorderLeft
            );
        }

        public LayoutBox GetBoxForElement(UIElement itemElement) {
            return m_LayoutBoxMap.GetOrDefault(itemElement.id);
        }

        public LightList<UIElement> GetVisibleElements(LightList<UIElement> retn = null) {
            if (retn == null) {
                retn = new LightList<UIElement>(m_VisibleBoxList.Count);
            }
            else {
                retn.EnsureCapacity(m_VisibleBoxList.Count);
            }

            UIElement[] elements = retn.Array;
            LayoutBox[] boxes = m_VisibleBoxList.Array;
            for (int i = 0; i < m_VisibleBoxList.Count; i++) {
                elements[i] = boxes[i].element;
            }

            retn.Count = m_VisibleBoxList.Count;
            return retn;
        }

        private static Extents GetLocalExtents(List<LayoutBox> children) {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                if (child.element.isDisabled) continue;

                Rect rect = child.element.layoutResult.LocalRect;
                Vector2 localPosition = new Vector2(rect.x, rect.y);

                if (localPosition.x < min.x) {
                    min.x = localPosition.x;
                }

                if (localPosition.y < min.y) {
                    min.y = localPosition.y;
                }

                if (localPosition.x + rect.width > max.x) {
                    max.x = localPosition.x + rect.width;
                }

                if (localPosition.y + rect.height > max.y) {
                    max.y = localPosition.y + rect.height;
                }
            }

            return new Extents(min, max);
        }

    }

}