﻿using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class LayoutSystem : ILayoutSystem {

        public struct ViewRect {

            public readonly UIView view;
            public readonly Rect previousViewport;

            public ViewRect(UIView view, Rect previousViewport) {
                this.view = view;
                this.previousViewport = previousViewport;
            }

        }

        public event Action<VirtualScrollbar> onCreateVirtualScrollbar;
        public event Action<VirtualScrollbar> onDestroyVirtualScrollbar;

        protected readonly IStyleSystem m_StyleSystem;
        protected readonly IntMap<LayoutBox> m_LayoutBoxMap;
        protected readonly LightList<VirtualElement> m_VirtualElements;
        protected readonly LightList<LayoutBox> m_PendingInitialization;

        private Size m_ScreenSize;
        private readonly LightList<UIElement> m_Elements;
        private readonly LightList<ViewRect> m_Views;
        private readonly LightList<UIElement> m_VisibleElements;

        private static readonly IComparer<UIElement> comparer = new UIElement.RenderLayerComparerAscending();

        public LayoutSystem(IStyleSystem styleSystem) {
            this.m_StyleSystem = styleSystem;
            this.m_LayoutBoxMap = new IntMap<LayoutBox>();
            this.m_Elements = new LightList<UIElement>();
            this.m_PendingInitialization = new LightList<LayoutBox>();
            this.m_VirtualElements = new LightList<VirtualElement>();
            this.m_Views = new LightList<ViewRect>();
            this.m_VisibleElements = new LightList<UIElement>();
            m_StyleSystem.onTextContentChanged += HandleTextContentChanged;
            m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        public void OnReset() {
            m_LayoutBoxMap.Clear();
            m_Elements.Clear();
            m_PendingInitialization.Clear();
            m_VirtualElements.Clear();
            m_Views.Clear();
        }

        protected void InitializeLayoutBoxes() {
            if (m_PendingInitialization.Count == 0) {
                return;
            }

            for (int i = 0; i < m_PendingInitialization.Count; i++) {
                LayoutBox box = m_PendingInitialization[i];
                box.OnInitialize();
                box.IsInitialized = true;
                box.markedForLayout = true;
            }

            m_PendingInitialization.Clear();
        }

        public void OnUpdate() {
            InitializeLayoutBoxes();

            // todo -- should this be a list per-view?
            m_VisibleElements.Clear();

            bool forceLayout = false;
            Size screen = new Size(Screen.width, Screen.height);
            if (m_ScreenSize != screen) {
                m_ScreenSize = screen;
                forceLayout = true;
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
            
            m_VisibleElements.Add(element);

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

            layoutResult.ActualSize = new Size(root.actualWidth, root.actualHeight);
            layoutResult.AllocatedSize = new Size(root.allocatedWidth, root.allocatedHeight);

            layoutResult.ContentRect = root.ContentRect;

            layoutResult.Scale = new Vector2(root.style.TransformScaleX, root.style.TransformScaleY);
            layoutResult.LocalPosition = ResolveLocalPosition(root);
            layoutResult.ScreenPosition = layoutResult.localPosition;
            layoutResult.Rotation = root.style.TransformRotation;
            layoutResult.clipRect = new Rect(0, 0, viewportRect.width, viewportRect.height);
            layoutResult.border = new OffsetRect(
                root.BorderTop,
                root.BorderRight,
                root.BorderBottom,
                root.BorderLeft
            );
            element.layoutResult = layoutResult;

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
#if DEBUG
                        box.layoutCalls++;
#endif
                    }

                    layoutResult = element.layoutResult;

                    LayoutBox parentBox = box.parent;
                    Vector2 scrollOffset = new Vector2();
                    scrollOffset.x = (parentBox.actualWidth - parentBox.allocatedWidth) * parentBox.element.scrollOffset.x;
                    scrollOffset.y = (parentBox.actualHeight - parentBox.allocatedHeight) * parentBox.element.scrollOffset.y;

                    layoutResult.LocalPosition = ResolveLocalPosition(box) - scrollOffset;
                    layoutResult.ContentRect = box.ContentRect;
                    layoutResult.ActualSize = new Size(box.actualWidth, box.actualHeight);
                    layoutResult.AllocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
                    layoutResult.ScreenPosition = box.parent.element.layoutResult.screenPosition + layoutResult.localPosition;
                    layoutResult.Scale = new Vector2(box.style.TransformScaleX, box.style.TransformScaleY);
                    layoutResult.Rotation = parentBox.style.TransformRotation + box.style.TransformRotation;
                    layoutResult.Pivot = box.Pivot;
                    layoutResult.border = new OffsetRect(
                        box.BorderTop,
                        box.BorderRight,
                        box.BorderBottom,
                        box.BorderLeft
                    );
                    // should be able to sort by view
                    Rect clipRect = new Rect(0, 0, viewportRect.width, viewportRect.height);
                    UIElement ptr = element.parent;
                    // find ancestor where layer is higher, might not be our parent

                    // todo -- handle non rect clip shapes: ie circle / ellipse

                    if (ptr != null) {
                        bool handlesHorizontal = ptr.style.OverflowX != Overflow.None;
                        bool handlesVertical = ptr.style.OverflowY != Overflow.None;
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

                    Rect screenRect = layoutResult.ScreenRect;
                    float clipX = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.x, screenRect.xMin, screenRect.xMax));
                    float clipY = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.y, screenRect.yMin, screenRect.yMax));
                    float clipW = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.xMax, screenRect.xMin, screenRect.xMax)) - clipWAdjustment;
                    float clipH = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.yMax, screenRect.yMin, screenRect.yMax)) - clipHAdjustment;

                    if (clipH <= 0 || clipW <= 0) {
                        cullResult = CullResult.ClipRectIsZero;
                    }

                    layoutResult.cullState = cullResult;
                    layoutResult.clipVector = new Vector4(clipX, clipY, clipW, clipH);

                    // todo actually use this
                    // if layout result size or position changed -> update the query grid
                    // if (layoutResult.PositionChanged || layoutResult.SizeChanged) {
                    //     UpdateQueryGrid(element, oldScreenRect);
                    // }

                    CreateOrDestroyScrollbars(box);
                    element.layoutResult = layoutResult;

                    stack.Push(element);
                    if (cullResult == CullResult.NotCulled) {
                        m_VisibleElements.Add(element);
                    }
                }
            }

            // TODO optimize this to only sort if styles changed, also our comparer is really slow right now
            
            m_VisibleElements.Sort(comparer);

            UIElement[] elements = m_VisibleElements.Array;
            for (int i = 0; i < m_VisibleElements.Count; i++) {
                UIElement e = elements[i];
                LayoutResult lr = e.layoutResult;
                lr.zIndex = (i + 1); 
                e.layoutResult = lr;
            }

            UpdateScrollbarLayouts();
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
                case LayoutBehavior.Ignored:

                    switch (transformBehaviorX) {
                        case TransformBehavior.AnchorMinOffset:
                            localPosition.x = box.TransformX;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.x = box.TransformX;
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
                            localPosition.y = box.TransformY;
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
                            localPosition.x = box.TransformX;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.x = box.TransformX;
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
                            localPosition.y = box.TransformY;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.y = box.TransformY;
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
            VirtualScrollbar vertical = box.verticalScrollbar;
            VirtualScrollbar horizontal = box.horizontalScrollbar;

            if (box.actualHeight <= box.allocatedHeight) {
                if (vertical != null) {
                    onDestroyVirtualScrollbar?.Invoke(vertical);
                    m_Elements.Remove(vertical);
                    m_VirtualElements.Remove(vertical);
                    box.verticalScrollbar = null; // todo -- pool
                }
            }
            else {
                Overflow verticalOverflow = box.style.OverflowY;

                if (vertical == null && verticalOverflow == Overflow.Scroll || verticalOverflow == Overflow.ScrollAndAutoHide) {
                    vertical = new VirtualScrollbar(element, ScrollbarOrientation.Vertical);
                    // todo -- depth index needs to be set

                    m_Elements.Add(vertical);

                    m_VirtualElements.Add(vertical);

                    Extents childExtents = GetLocalExtents(box.children);
                    float offsetY = (childExtents.min.y < 0) ? -childExtents.min.y / box.allocatedHeight : 0f;
                    element.scrollOffset = new Vector2(element.scrollOffset.x, offsetY);

                    onCreateVirtualScrollbar?.Invoke(vertical);
                    box.verticalScrollbar = vertical;
                }
            }

            if (box.actualWidth <= box.allocatedWidth) {
                if (horizontal != null) {
                    onDestroyVirtualScrollbar?.Invoke(horizontal);
                    m_Elements.Remove(horizontal);
                    m_VirtualElements.Remove(horizontal);
                    box.horizontalScrollbar = null; // todo -- pool
                }
            }
            else {
                Overflow horizontalOverflow = box.style.OverflowX;
                if (horizontal == null && horizontalOverflow == Overflow.Scroll || horizontalOverflow == Overflow.ScrollAndAutoHide) {
                    horizontal = new VirtualScrollbar(element, ScrollbarOrientation.Horizontal);
                    // todo -- depth index needs to be set

                    m_Elements.Add(horizontal);

                    m_VirtualElements.Add(horizontal);

                    Extents childExtents = GetLocalExtents(box.children);
                    float offsetX = (childExtents.min.x < 0) ? -childExtents.min.x / box.allocatedWidth : 0f;
                    element.scrollOffset = new Vector2(element.scrollOffset.y, offsetX);

                    onCreateVirtualScrollbar?.Invoke(horizontal);
                    box.horizontalScrollbar = horizontal;
                }
            }
        }

        private void UpdateScrollbarLayouts() {
            for (int i = 0; i < m_VirtualElements.Count; i++) {
                VirtualScrollbar scrollbar = (VirtualScrollbar) m_VirtualElements[i];
                scrollbar.RunLayout();

                LayoutResult scrollbarResult = scrollbar.layoutResult;
                LayoutResult targetResult = scrollbar.targetElement.layoutResult;

                Rect trackRect = scrollbar.trackRect;
                scrollbarResult.zIndex = 999999;
                scrollbarResult.localPosition = new Vector2(trackRect.x, trackRect.y);
                scrollbarResult.screenPosition = targetResult.screenPosition + scrollbarResult.localPosition;
                scrollbarResult.clipRect = targetResult.clipRect;
                scrollbarResult.actualSize = new Size(trackRect.width, trackRect.height);
                scrollbarResult.allocatedSize = scrollbarResult.actualSize;

                scrollbar.layoutResult = scrollbarResult;
            }
        }

        private void LayoutSticky() {
            // only sticky within the parent
            // Layer = 1
            // ZIndex = 1
            // PreferredWidth = new UIMeasurement(1f, UIUnit.AnchorWidth | UIUnit.AnchorHeight);
            // AnchorTop = UIFixedLength
            // AnchorRight
            // AnchorBottom
            // AnchorLeft
            // AnchorTarget = Viewport | Screen | Parent | Template?  
            // TransformPositionXBehavior = Ignore | LayoutOffset | Normal | Anchor Offset
            // TranslateX = new UIFixedLength(-1f, Percent);
            // TransformPositionXAnchor

            // TransformPositionXBehavior =  LayoutOffset | Default | Fixed | Sticky;

            // LayoutBehavior = Normal | Anchor | Fixed | Sticky
            // TransformAnchorLeft = new UIAnchor(value, Left | Right);
            // TransformAnchorY = Top | Bottom
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
                        if (property.AsLayoutBehavior == LayoutBehavior.Ignored) {
                            box.parent?.OnChildDisabled(box);
                        }
                        else {
                            box.parent?.OnChildEnabled(box);
                        }

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

        // todo -- remove, let textbox handle this
        private void HandleTextContentChanged(UIElement element, string content) {
            LayoutBox box;
            if (m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                TextLayoutBox textBox = box as TextLayoutBox;
                textBox?.OnTextContentUpdated(); // todo -- remove this
            }
        }

        private void HandleLayoutChanged(UIElement element) {
            LayoutBox box;
            if (!m_LayoutBoxMap.TryGetValue(element.id, out box)) {
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
            LayoutBox child = m_LayoutBoxMap.GetOrDefault(element.id);
            Stack<UIElement> elements = StackPool<UIElement>.Get();
            LightList<LayoutBox> boxes = LightListPool<LayoutBox>.Get();

            elements.Push(element);
            while (elements.Count != 0) {
                UIElement current = elements.Pop();

                LayoutBox box = m_LayoutBoxMap.GetOrDefault(current.id);

                if (box == null) continue;

                box.markedForLayout = true;

                if (current.children == null) {
                    continue;
                }

                box.InvalidatePreferredSizeCache();
                boxes.Clear();
                boxes.EnsureCapacity(current.children.Count);
                for (int i = 0; i < current.children.Count; i++) {
                    UIElement childElement = current.children[i];
                    elements.Push(childElement);
                    if (childElement.isDisabled || childElement.style.LayoutBehavior == LayoutBehavior.Ignored) {
                        continue;
                    }

                    LayoutBox childBox = m_LayoutBoxMap.GetOrDefault(childElement.id);
                    boxes.AddUnchecked(childBox);
                }

                box.SetChildren(boxes);
            }

            if (child.parent != null && child.element.style.LayoutBehavior != LayoutBehavior.Ignored) {
                child.parent.OnChildEnabled(child);
                child.parent.RequestContentSizeChangeLayout();
            }

            LightListPool<LayoutBox>.Release(ref boxes);
        }


        public void OnElementDisabled(UIElement element) {
            LayoutBox child = m_LayoutBoxMap.GetOrDefault(element.id);
            if (child?.parent != null) {
                child.parent.OnChildDisabled(child);
                child.parent.RequestContentSizeChangeLayout();
            }
        }

        public void OnElementDestroyed(UIElement element) {
            LayoutBox child = m_LayoutBoxMap.GetOrDefault(element.id);

            // todo destroy scroll bars

            if (child?.parent != null) {
                child.parent.OnChildDisabled(child);
                child.parent.RequestContentSizeChangeLayout();
            }

            m_Elements.Remove(element);
            m_PendingInitialization.Remove(child);
            m_LayoutBoxMap.Remove(element.id);

            // todo -- maybe recycle the layout box

            if (element.children != null) {
                for (int i = 0; i < element.children.Count; i++) {
                    OnElementDestroyed(element.children[i]);
                }
            }
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) {
            
        }

        // todo pool boxes
        private LayoutBox CreateLayoutBox(UIElement element) {
            if ((element is UITextElement)) {
                return new TextLayoutBox(element);
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

            if (element.parent != null) {
                layoutBox.SetParent(m_LayoutBoxMap.GetOrDefault(element.parent.id));
            }

            m_LayoutBoxMap.Add(element.id, layoutBox);
            stack.Push(ValueTuple.Create(element, layoutBox));

            m_Elements.Add(element);
            m_PendingInitialization.Add(layoutBox);

            while (stack.Count > 0) {
                ValueTuple<UIElement, LayoutBox> item = stack.Pop();
                UIElement parentElement = item.Item1;
                LayoutBox parentBox = item.Item2;

                if (parentBox == null) {
                    LayoutBox ptr = null;
                    UIElement e = parentElement;
                    while (ptr == null) {
                        e = e.parent;
                        ptr = m_LayoutBoxMap.GetOrDefault(e.id);
                    }

                    parentBox = ptr;
                }

                if (parentElement.children == null) {
                    continue;
                }

                for (int i = 0; i < parentElement.children.Count; i++) {
                    UIElement child = parentElement.children[i];
                    LayoutBox childBox = CreateLayoutBox(child);
                    childBox.SetParent(parentBox);
                    m_LayoutBoxMap.Add(child.id, childBox);
                    stack.Push(ValueTuple.Create(child, childBox));
                    m_Elements.Add(child);
                    m_PendingInitialization.Add(childBox);
                }
            }

            StackPool<ValueTuple<UIElement, LayoutBox>>.Release(stack);
        }

        public List<UIElement> QueryPoint(Vector2 point, List<UIElement> retn) {
            // todo if point is same as last point or point is off screen, do no work
            if (retn == null) {
                retn = ListPool<UIElement>.Get();
            }

            UIElement[] elements = m_VisibleElements.Array;
            int elementCount = m_VisibleElements.Count;
            for (int i = 0; i < elementCount; i++) {
                UIElement element = elements[i];
                LayoutResult layoutResult = element.layoutResult;

                // todo make this better
                if (element.isDisabled || !layoutResult.ScreenRect.ContainOrOverlap(point)) {
                    continue;
                }

                UIElement ptr = element.parent;
                while (ptr != null) {
                    Vector2 screenPosition = ptr.layoutResult.screenPosition;
                    if (ptr.style.OverflowX != Overflow.None) {
                        if (point.x < screenPosition.x || point.x > screenPosition.x + ptr.layoutResult.AllocatedWidth) {
                            break;
                        }
                    }

                    if (ptr.style.OverflowY != Overflow.None) {
                        if (point.y < screenPosition.y || point.y > screenPosition.y + ptr.layoutResult.AllocatedHeight) {
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

        public LightList<UIElement> GetVisibleElements() {
            return m_VisibleElements;
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