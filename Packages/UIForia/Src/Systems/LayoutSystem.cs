﻿using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public struct LayoutData {

        public UIFixedLength paddingTop;
        public UIFixedLength paddingRight;
        public UIFixedLength paddingBottom;
        public UIFixedLength paddingLeft;
        
        public UIFixedLength borderTop;
        public UIFixedLength borderRight;
        public UIFixedLength borderBottom;
        public UIFixedLength borderLeft;
        
        public UIMeasurement marginTop;
        public UIMeasurement marginRight;
        public UIMeasurement marginBottom;
        public UIMeasurement marginLeft;

        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public Matrix4x4 matrix;
        public RenderLayer renderLayer;
        public int zIndex;

        public UIMeasurement prefWidth;
        public UIMeasurement minWidth;
        public UIMeasurement maxWidth;

        public UIMeasurement prefHeight;
        public UIMeasurement minHeight;
        public UIMeasurement maxHeight;

        public Vector2 pivot;
        public Vector2 transformPosition;
        public TransformBehavior transformBehaviorX;
        public TransformBehavior transformBehaviorY;

    }
    
    public class LayoutSystem : ILayoutSystem {

        public struct ViewRect {

            public readonly UIView view;
            public readonly Rect previousViewport;

            public ViewRect(UIView view, Rect previousViewport) {
                this.view = view;
                this.previousViewport = previousViewport;
            }

        }

        protected readonly IStyleSystem m_StyleSystem;
        protected readonly IntMap<LayoutBox> m_LayoutBoxMap;
        protected readonly LightList<TextLayoutBox> m_TextLayoutBoxes;

        private Size m_ScreenSize;
        private readonly LightList<ViewRect> m_Views;
        private readonly LightList<UIElement> m_VisibleElementList;

        private static readonly IComparer<UIElement> comparer = new UIElement.RenderLayerComparerAscending();

        public LayoutSystem(IStyleSystem styleSystem) {
            this.m_StyleSystem = styleSystem;
            this.m_LayoutBoxMap = new IntMap<LayoutBox>();
            this.m_Views = new LightList<ViewRect>();
            this.m_VisibleElementList = new LightList<UIElement>();
            this.m_TextLayoutBoxes = new LightList<TextLayoutBox>(64);
            this.m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        public void OnReset() {
            m_LayoutBoxMap.Clear();
            m_VisibleElementList.Clear();
            m_Views.Clear();
        }

        public void OnUpdate() {
            // todo -- should this be a list per-view?
            m_VisibleElementList.Clear();

            bool forceLayout = false;
            Size screen = new Size(Screen.width, Screen.height);
            if (m_ScreenSize != screen) {
                m_ScreenSize = screen;
                forceLayout = true;
            }

            TextLayoutBox[] textLayouts = m_TextLayoutBoxes.Array;
            for (int i = 0; i < m_TextLayoutBoxes.Count; i++) {
                if (textLayouts[i].TextInfo.LayoutDirty) {
                    textLayouts[i].RequestContentSizeChangeLayout();
                }
            }

            for (int i = 0; i < m_Views.Count; i++) {
                RunLayout(forceLayout, m_Views[i]);
                m_Views[i] = new ViewRect(m_Views[i].view, m_Views[i].view.Viewport);
            }
        }

        public void RunLayout(bool forceLayout, ViewRect viewRect) {
            Rect rect = viewRect.previousViewport;
            UIView view = viewRect.view;
            Rect viewportRect = view.Viewport;

            LayoutBox root = m_LayoutBoxMap.GetOrDefault(view.RootElement.id);

            if (rect != view.Viewport) {
                root.allocatedWidth = Mathf.Min(root.GetWidths().clampedSize, view.Viewport.width);
                root.allocatedHeight = Mathf.Min(root.GetHeights(root.allocatedWidth).clampedSize, view.Viewport.height);
                root.markedForLayout = true;
            }

            Stack<UIElement> stack = StackPool<UIElement>.Get();

            // if we don't allow reparenting, could just use a flat sorted list
            // as long as the parent is laid out before the child that should be fine

            UIElement element = view.RootElement;
            LayoutResult layoutResult = element.layoutResult;
            stack.Push(element);

            m_VisibleElementList.Add(element);

            if (root.IsIgnored) {
                root.allocatedWidth = root.GetWidths().clampedSize;
                root.allocatedHeight = root.GetHeights(root.allocatedWidth).clampedSize;
            }

            if (forceLayout || root.markedForLayout) {
                root.RunLayout();
                root.markedForLayout = false;
#if DEBUG
                root.layoutCalls++;
#endif
            }

            // actual size should probably be the root containing all children, ignored or not

            layoutResult.actualSize = new Size(root.actualWidth, root.actualHeight);
            layoutResult.allocatedSize = new Size(root.allocatedWidth, root.allocatedHeight);

            layoutResult.ContentRect = root.ContentRect;

            layoutResult.scale = new Vector2(root.style.TransformScaleX, root.style.TransformScaleY);
            layoutResult.localPosition = ResolveLocalPosition(root);
            layoutResult.screenPosition = layoutResult.localPosition;
            layoutResult.rotation = root.style.TransformRotation;
            layoutResult.clipRect = new Rect(0, 0, viewportRect.width, viewportRect.height);
            layoutResult.border = new OffsetRect(
                root.BorderTop,
                root.BorderRight,
                root.BorderBottom,
                root.BorderLeft
            );

            layoutResult.padding = new OffsetRect(
                root.PaddingTop,
                root.PaddingRight,
                root.PaddingBottom,
                root.PaddingLeft
            );

            layoutResult.matrix = SVGXMatrix.TRS(
                new Vector2(root.TransformX, root.TransformY),
                root.style.TransformRotation,
                new Vector2(root.style.TransformScaleX, root.style.TransformScaleY)
            );

            CreateOrDestroyScrollbars(root);

            while (stack.Count > 0) {
                UIElement current = stack.Pop();

                if (!current.isEnabled) {
                    continue;
                }

                if (current.children == null) {
                    continue;
                }

                for (int i = 0; i < current.children.Count; i++) {
                    element = current.children[i];

                    LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);

                    if (!element.isEnabled) {
                        continue;
                    }

                    if (box == null) {
                        stack.Push(element);
                        continue;
                    }

                    if (box.IsIgnored) {
                        float currentWidth = box.allocatedWidth;
                        float currentHeight = box.allocatedHeight;
                        box.allocatedWidth = box.GetWidths().clampedSize;
                        box.allocatedHeight = box.GetHeights(box.actualHeight).clampedSize;

                        if (box.allocatedWidth != currentWidth || box.allocatedHeight != currentHeight) {
                            box.markedForLayout = true;
                        }
                    }

                    if (forceLayout || box.markedForLayout) {
                        box.RunLayout();
                        box.markedForLayout = false;
                        CreateOrDestroyScrollbars(box);
#if DEBUG
                        box.layoutCalls++;
#endif
                    }

                    layoutResult = element.layoutResult;

                    LayoutBox parentBox = box.parent;

                    Vector2 scrollOffset = new Vector2();
                    scrollOffset.x = (parentBox.actualWidth - parentBox.allocatedWidth) * parentBox.element.scrollOffset.x;
                    scrollOffset.y = (parentBox.actualHeight - parentBox.allocatedHeight) * parentBox.element.scrollOffset.y;

                    layoutResult.localPosition = ResolveLocalPosition(box) - scrollOffset;
                    layoutResult.ContentRect = box.ContentRect;
                    layoutResult.actualSize = new Size(box.actualWidth, box.actualHeight);
                    layoutResult.allocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
                    layoutResult.screenPosition = parentBox.element.layoutResult.screenPosition + layoutResult.localPosition;
                    layoutResult.scale = new Vector2(box.style.TransformScaleX, box.style.TransformScaleY); // only set if changed
                    layoutResult.rotation = parentBox.style.TransformRotation + box.style.TransformRotation; // only set if changed
                    layoutResult.pivot = box.Pivot; // only set if changed

                    layoutResult.borderRadius = new ResolvedBorderRadius(
                        box.BorderRadiusTopLeft,
                        box.BorderRadiusTopRight,
                        box.BorderRadiusBottomRight,
                        box.BorderRadiusBottomLeft
                    );

                    layoutResult.border = new OffsetRect( // only set if changed
                        box.BorderTop,
                        box.BorderRight,
                        box.BorderBottom,
                        box.BorderLeft
                    );

                    layoutResult.padding = new OffsetRect(
                        box.PaddingTop,
                        box.PaddingRight,
                        box.PaddingBottom,
                        box.PaddingLeft
                    );

                    SVGXMatrix m = SVGXMatrix.TRS(layoutResult.localPosition, layoutResult.rotation, layoutResult.scale);
                    Vector2 pivot = box.Pivot;
                    Vector2 offset = new Vector2(layoutResult.allocatedSize.width * pivot.x, layoutResult.allocatedSize.height * pivot.y);
                    SVGXMatrix parentMatrix = box.parent.element.layoutResult.matrix;
                    SVGXMatrix pivotMat = SVGXMatrix.identity.Translate(offset);
                    SVGXMatrix minusPivotMat = pivotMat.Inverse();

                    m = pivotMat * m * minusPivotMat;
                    m = parentMatrix * m;
                    layoutResult.matrix = m;

                    // should be able to sort by view
                    Rect clipRect = new Rect(0, 0, viewportRect.width, viewportRect.height);
                    UIElement ptr = element.parent;
                    // find ancestor where layer is higher, might not be our parent

                    // todo -- handle non rect clip shapes: ie circle / ellipse

                    if (ptr != null) {
                        bool handlesHorizontal = ptr.style.OverflowX != Overflow.Visible;
                        bool handlesVertical = ptr.style.OverflowY != Overflow.Visible;
                        if (handlesHorizontal && handlesVertical) {
                            Rect r = new Rect(ptr.layoutResult.screenPosition, ptr.layoutResult.allocatedSize);
                            clipRect = clipRect.Intersect(r.Intersect(ptr.layoutResult.clipRect));
                        }
                        else if (handlesHorizontal) {
                            Rect r = new Rect(
                                ptr.layoutResult.screenPosition.x,
                                ptr.layoutResult.clipRect.y,
                                ptr.layoutResult.AllocatedWidth,
                                ptr.layoutResult.clipRect.height
                            );
                            clipRect = r.Intersect(clipRect);
                        }
                        else if (handlesVertical) {
                            Rect r = new Rect(
                                ptr.layoutResult.clipRect.x,
                                ptr.layoutResult.screenPosition.y,
                                ptr.layoutResult.clipRect.width,
                                ptr.layoutResult.AllocatedHeight
                            );
                            clipRect = r.Intersect(clipRect);
                        }
                        else {
                            clipRect = ptr.layoutResult.clipRect;
                        }
                    }

                    layoutResult.clipRect = clipRect;

                    Rect intersectedClipRect = layoutResult.clipRect.Intersect(layoutResult.ScreenRect);
                    CullResult cullResult = CullResult.NotCulled;

                    float clipWAdjustment = 0;
                    float clipHAdjustment = 0;

                    if (intersectedClipRect.width <= 0 || intersectedClipRect.height <= 0) {
                        cullResult = CullResult.ClipRectIsZero;
                    }
                    else if (layoutResult.actualSize.width * layoutResult.actualSize.height <= 0) {
                        cullResult = CullResult.ActualSizeZero;
                    }
                    else if (layoutResult.allocatedSize.height < layoutResult.actualSize.height) {
                        clipHAdjustment = 1 - (layoutResult.allocatedSize.height / layoutResult.actualSize.height);
                        if (clipHAdjustment >= 1) {
                            cullResult = CullResult.ClipRectIsZero;
                        }
                    }
                    else if (layoutResult.allocatedSize.width < layoutResult.actualSize.width) {
                        clipWAdjustment = 1 - (layoutResult.allocatedSize.width / layoutResult.actualSize.width);
                        if (clipWAdjustment >= 1) {
                            cullResult = CullResult.ClipRectIsZero;
                        }
                    }

                    // todo -- can i get rid of clip vector here?
                    Rect screenRect = layoutResult.ScreenRect;
                    float clipW = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.xMax, screenRect.xMin, screenRect.xMax)) - clipWAdjustment;
                    float clipH = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.yMax, screenRect.yMin, screenRect.yMax)) - clipHAdjustment;

                    if (clipH <= 0 || clipW <= 0) {
                        cullResult = CullResult.ClipRectIsZero;
                    }

                    layoutResult.cullState = cullResult;

                    // todo actually use this
                    // if layout result size or position changed -> update the query grid
                    // if (layoutResult.PositionChanged || layoutResult.SizeChanged) {
                    //     UpdateQueryGrid(element, oldScreenRect);
                    // }

                    stack.Push(element);
                    if (cullResult == CullResult.NotCulled) {
                        m_VisibleElementList.Add(element);
                    }
                }
            }

            // TODO optimize this to only sort if styles changed, also our comparer is really slow right now

            m_VisibleElementList.Sort(comparer);

            UIElement[] elements = m_VisibleElementList.Array;
            for (int i = 0; i < m_VisibleElementList.Count; i++) {
                elements[i].layoutResult.zIndex = i + 1;
            }

            StackPool<UIElement>.Release(stack);
        }

        private void UpdateQueryGrid(UIElement element, Rect oldRect) {
            // todo this should replace the brute force search for querying
            int x = (int) oldRect.x;
            int y = (int) oldRect.y;
            int w = (int) oldRect.width;
            int h = (int) oldRect.height;
            x = (x / 100) * 100;
            y = (y / 100) * 100;
            w = (w / 100) * 100;
            h = (h / 100) * 100;
            int horizontalBlockCount = w / 100;
            int verticalBlockCount = h / 100;
            // note: x and y can be negative!
            // round to nearest 100
            // assume screen divided into n blocks of 100
            // each block addressable by x/y in single int
            // each block has set of members
            // shift as needed
            int start = BitUtil.SetHighLowBits((int) oldRect.x, (int) oldRect.y);
            int end = BitUtil.SetHighLowBits((int) (oldRect.x + oldRect.width), (int) (oldRect.y + oldRect.height));
        }

        private static Vector2 ResolveLocalPosition(LayoutBox box) {
            Vector2 localPosition = Vector2.zero;

            LayoutBehavior layoutBehavior = box.style.LayoutBehavior;
            TransformBehavior transformBehaviorX = box.style.TransformBehaviorX;
            TransformBehavior transformBehaviorY = box.style.TransformBehaviorY;

            switch (layoutBehavior) {
                case LayoutBehavior.TranscludeChildren:
                    localPosition = new Vector2(0, 0);
                    break;

                case LayoutBehavior.Ignored:
                    
                    // todo verify these visually something is wrong
                    switch (transformBehaviorX) {
                        case TransformBehavior.AnchorMinOffset:
                            localPosition.x = box.AnchorLeft + box.TransformX;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.x = box.AnchorRight - box.parent.element.layoutResult.screenPosition.x - box.TransformX - box.actualWidth;
                            break;
                        case TransformBehavior.LayoutOffset:
                            localPosition.x = box.TransformX;
                            break;
                        default:
                            localPosition.x = box.TransformX;
                            break;
                    }

                    switch (transformBehaviorY) {
                        case TransformBehavior.AnchorMinOffset:
                            localPosition.y = box.TransformY;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.y = box.AnchorBottom - box.parent.element.layoutResult.screenPosition.y - box.TransformY - box.actualHeight;
                            break;
                        case TransformBehavior.LayoutOffset:
                            localPosition.y = box.TransformY;
                            break;
                        default:
                            localPosition.y = box.localY;
                            break;
                    }

                    break;

                case LayoutBehavior.Normal:
                    switch (transformBehaviorX) {
                        case TransformBehavior.AnchorMinOffset:
                            localPosition.x = box.AnchorLeft - box.parent.element.layoutResult.screenPosition.x + box.TransformX;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.x = box.AnchorRight - box.parent.element.layoutResult.screenPosition.x - box.TransformX - box.actualWidth;

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
                            localPosition.y = box.AnchorTop - box.parent.element.layoutResult.screenPosition.y + box.TransformY;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.y = box.AnchorBottom - box.parent.element.layoutResult.screenPosition.y - box.TransformY - box.actualHeight;
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
            m_Views.Add(new ViewRect(view, new Rect()));
        }

        public void OnViewRemoved(UIView view) {
//            m_Views.Remove(view);
        }

        private void HandleStylePropertyChanged(UIElement element, LightList<StyleProperty> properties) {
            // todo early-out if we haven't had a layout pass for the element yet
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) {
                return;
            }

            bool notifyParent = box.parent != null && (box.style.LayoutBehavior & LayoutBehavior.Ignored) == 0 && box.element.isEnabled;
            bool invalidatePreferredSizeCache = false;

            for (int i = 0; i < properties.Count; i++) {
                StyleProperty property = properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.LayoutBehavior:
                        // todo -- implement this
                        box.UpdateChildren();

                        break;
                    case StylePropertyId.LayoutType:
                        HandleLayoutChanged(element);
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

        private void HandleLayoutChanged(UIElement element) {
            LayoutBox box;
            if (!m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                return;
            }

            if ((box.element.flags & UIElementFlags.TextElement) != 0) {
                return;
            }

            LayoutBox parent = box.parent;
            LayoutBox replace = box;

            switch (element.style.LayoutType) {
                case LayoutType.Radial:
                    if (!(box is RadialLayoutBox)) {
                        replace = new RadialLayoutBox(element);
                    }

                    break;
                case LayoutType.Fixed:
                    if (!(box is FixedLayoutBox)) {
                        replace = new FixedLayoutBox(element);
                    }

                    break;
                case LayoutType.Flex:
                    if (!(box is FlexLayoutBox)) {
                        replace = new FlexLayoutBox(element);
                    }

                    break;
                case LayoutType.Grid:
                    if (!(box is GridLayoutBox)) {
                        replace = new GridLayoutBox(element);
                    }

                    break;
                case LayoutType.Flow:
                    if (!(box is FlowLayoutBox)) {
                        replace = new FlowLayoutBox(element);
                    }

                    break;
            }

            if (replace != box) {
                parent?.ReplaceChild(box, replace);
                // release(box)
                m_LayoutBoxMap[element.id] = replace;
            }

            // if removed -> release box
            // update map to hold new box
        }

        public void OnElementEnabled(UIElement element) {
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) return; // can happen if disable is called in binding before layout system gets the create call

            if (box.parent != null) {
                UpdateChildrenRecursive(box.parent.element);
            }
        }

        public void OnElementDisabled(UIElement element) {
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) return; // can happen if disable is called in binding before layout system gets the create call 
            if (box.parent != null) {
                UpdateChildren(box.parent);
            }

            m_VisibleElementList.Remove(element);
        }

        private void UpdateChildrenRecursive(UIElement element) {
            if (element.children == null) return;

            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);

            for (int i = 0; i < element.children.Count; i++) {
                UpdateChildrenRecursive(element.children[i]);
            }

            UpdateChildren(box);
            box.UpdateChildren();
        }

        public void OnElementDestroyed(UIElement element) {
            UpdateChildren(m_LayoutBoxMap.GetOrDefault(element.id));
            m_VisibleElementList.Remove(element);
            m_LayoutBoxMap.Remove(element.id);
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        // todo pool boxes
        private LayoutBox CreateLayoutBox(UIElement element) {
            if ((element is UITextElement)) {
                TextLayoutBox textLayout = new TextLayoutBox(element);
                m_TextLayoutBoxes.Add(textLayout);
                return textLayout;
            }

            if ((element is UIImageElement)) {
                return new ImageLayoutBox(element);
            }

            switch (element.style.LayoutType) {
                case LayoutType.Flex:
                    return new FlexLayoutBox(element);

                case LayoutType.Flow:
                    return new FlowLayoutBox(element);

                case LayoutType.Fixed:
                    return new FixedLayoutBox(element);

                case LayoutType.Grid:
                    return new GridLayoutBox(element);

                case LayoutType.Radial:
                    return new RadialLayoutBox(element);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnElementCreated(UIElement element) {
            LayoutBox layoutBox = CreateLayoutBox(element);
            Stack<ValueTuple<UIElement, LayoutBox>> stack = StackPool<ValueTuple<UIElement, LayoutBox>>.Get();
            LightList<LayoutBox> toUpdateList = LightListPool<LayoutBox>.Get();

            if (element.parent != null) {
                layoutBox.parent = m_LayoutBoxMap.GetOrDefault(element.parent.id);
            }

            m_LayoutBoxMap.Add(element.id, layoutBox);
            stack.Push(ValueTuple.Create(element, layoutBox));

            while (stack.Count > 0) {
                ValueTuple<UIElement, LayoutBox> item = stack.Pop();
                UIElement parentElement = item.Item1;
                LayoutBox parentBox = item.Item2;

                toUpdateList.Add(parentBox);

                if (parentElement.children == null) {
                    continue;
                }

                for (int i = 0; i < parentElement.children.Count; i++) {
                    UIElement child = parentElement.children[i];
                    LayoutBox childBox = CreateLayoutBox(child);
                    childBox.parent = parentBox; // will get overridden for transcluded behaviors
                    m_LayoutBoxMap.Add(child.id, childBox);
                    stack.Push(ValueTuple.Create(child, childBox));
                }
            }

            int count = toUpdateList.Count;
            LayoutBox[] toUpdate = toUpdateList.Array;

            for (int i = 0; i < count; i++) {
                UpdateChildren(toUpdate[i]);
            }

            LightListPool<LayoutBox>.Release(ref toUpdateList);
            StackPool<ValueTuple<UIElement, LayoutBox>>.Release(stack);

            if (element.parent != null) {
                LayoutBox ptr = layoutBox.parent;
                while (ptr != null) {
                    if (ptr.style.LayoutBehavior != LayoutBehavior.TranscludeChildren) {
                        UpdateChildren(ptr);
                        break;
                    }

                    ptr = ptr.parent;
                }
            }
        }

        private void GetChildBoxes(LayoutBox box, LightList<LayoutBox> list) {
            UIElement element = box.element;
            UIElement[] children = element.children.Array;
            int count = element.children.Count;

            for (int i = 0; i < count; i++) {
                LayoutBox childBox = m_LayoutBoxMap[children[i].id];
                if (childBox.element.isDisabled) {
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
            // todo if point is same as last point or point is off screen, do no work
            if (retn == null) {
                retn = ListPool<UIElement>.Get();
            }

            UIElement[] elements = m_VisibleElementList.Array;
            int elementCount = m_VisibleElementList.Count;
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

            return retn;
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
                retn = new LightList<UIElement>(m_VisibleElementList.Count);
            }
            else {
                retn.EnsureCapacity(m_VisibleElementList.Count);
            }

            m_VisibleElementList.CopyTo(retn.Array, 0);
            retn.Count = m_VisibleElementList.Count;
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