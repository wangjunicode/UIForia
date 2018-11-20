using System;
using System.Collections.Generic;
using UIForia.Extensions;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

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

        // todo -- use arrays instead
        protected readonly IStyleSystem m_StyleSystem;
        protected readonly IntMap<UIElement[]> m_QueryGrid;
        protected readonly IntMap<LayoutBox> m_LayoutBoxMap;
        protected readonly List<VirtualElement> m_VirtualElements;
        protected readonly List<LayoutBox> m_PendingInitialization;

        private Size m_ScreenSize;
        private readonly List<UIElement> m_Elements;
        private readonly LightList<ViewRect> m_Views;
    
        public LayoutSystem(IStyleSystem styleSystem) {
            this.m_StyleSystem = styleSystem;
            this.m_LayoutBoxMap = new IntMap<LayoutBox>();
            this.m_Elements = new List<UIElement>();
            this.m_PendingInitialization = new List<LayoutBox>();
            this.m_VirtualElements = new List<VirtualElement>();
            this.m_QueryGrid = new IntMap<UIElement[]>();
            this.m_Views = new LightList<ViewRect>();
            m_StyleSystem.onTextContentChanged += HandleTextContentChanged;
            m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        public void OnReset() {
            m_LayoutBoxMap.Clear();
            m_Elements.Clear();
            m_PendingInitialization.Clear();
            m_VirtualElements.Clear();
            m_QueryGrid.Clear();
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

            // todo -- use the skip tree! use it!
            // if we don't allow reparenting, could just use a flat sorted list
            // as long as the parent is laid out before the child that should be fine

            UIElement element = view.RootElement;
            LayoutResult layoutResult = element.layoutResult;
            stack.Push(element);

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

            int depth = 0;
            int computedLayer = ResolveRenderLayer(element) - element.style.RenderLayerOffset;
            int zIndex = element.style.ZIndex;

            if (depth > computedLayer) {
                zIndex -= 2000;
            }
            else if (depth < computedLayer) {
                zIndex += -2000;
            }

            // actual size should probably be the root containing all children, ignored or not

            layoutResult.ActualSize = new Size(root.actualWidth, root.actualHeight);
            layoutResult.AllocatedSize = new Size(root.allocatedWidth, root.allocatedHeight);

            layoutResult.ContentRect = root.ContentRect;

            layoutResult.Scale = new Vector2(root.style.TransformScaleX, root.style.TransformScaleY);
            layoutResult.LocalPosition = ResolveLocalPosition(root);
            layoutResult.ScreenPosition = layoutResult.localPosition;
            layoutResult.Rotation = root.style.TransformRotation;
            layoutResult.Layer = computedLayer;
            layoutResult.ZIndex = zIndex;
            layoutResult.clipRect = new Rect(0, 0, viewportRect.width, viewportRect.height);
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

                for (int i = 0; i < current.children.Length; i++) {
                    element = current.children[i];

                    LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);

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

                    depth = element.depth;
                    computedLayer = ResolveRenderLayer(element) - element.style.RenderLayerOffset;
                    zIndex = element.style.ZIndex;

                    if (depth > computedLayer) {
                        zIndex -= 2000;
                    }
                    else if (depth < computedLayer) {
                        zIndex += -2000;
                    }

                    layoutResult = element.layoutResult;
                    Rect oldScreenRect = layoutResult.ScreenRect;

                    LayoutBox parentBox = box.parent;
                    Vector2 scrollOffset = new Vector2();
                    scrollOffset.x = (parentBox.actualWidth - parentBox.allocatedWidth) * parentBox.element.scrollOffset.x;
                    scrollOffset.y = (parentBox.actualHeight - parentBox.allocatedHeight) * parentBox.element.scrollOffset.y;

                    layoutResult.LocalPosition = ResolveLocalPosition(box) - scrollOffset;
                    layoutResult.ContentRect = box.ContentRect; // = new Vector2(box.ContentOffsetLeft, box.ContentOffsetTop);
                    layoutResult.ActualSize = new Size(box.actualWidth, box.actualHeight);
                    layoutResult.AllocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
                    layoutResult.ScreenPosition = box.parent.element.layoutResult.screenPosition + layoutResult.localPosition;
                    layoutResult.Scale = new Vector2(box.style.TransformScaleX, box.style.TransformScaleY);
                    layoutResult.Rotation = parentBox.style.TransformRotation + box.style.TransformRotation;
                    layoutResult.Pivot = box.Pivot;
                    layoutResult.Layer = computedLayer;
                    layoutResult.ZIndex = zIndex;

                    // should be able to sort by view
                    Rect clipRect = new Rect(0, 0, viewportRect.width, viewportRect.height);
                    UIElement ptr = element.parent;
                    // find ancestor where layer is higher, might not be our parent

                    // while parent is higher layer and requires layout
                    while (ptr != null) {
                        if (((ptr.flags & UIElementFlags.RequiresLayout) == 0)) {
                            ptr = ptr.parent;
                            continue;
                        }

                        if (ptr.layoutResult.layer < computedLayer) {
                            break;
                        }

                        ptr = ptr.parent;
                    }

                    if (ptr != null) {
                        bool handlesHorizontal = ptr.style.OverflowX != Overflow.None;
                        bool handlesVertical =  ptr.style.OverflowY != Overflow.None;
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
                    element.layoutResult = layoutResult;

                    // if layout result size or position changed -> update the query grid
                    if (layoutResult.PositionChanged || layoutResult.SizeChanged) {
                        UpdateQueryGrid(element, oldScreenRect);
                    }

                    CreateOrDestroyScrollbars(box);

                    stack.Push(element);
                }
            }

            UpdateScrollbarLayouts();
            StackPool<UIElement>.Release(stack);
        }

        private void UpdateQueryGrid(UIElement element, Rect oldRect) {
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
                    if (transformBehaviorX == TransformBehavior.AnchorMinOffset) {
                        localPosition.x = box.AnchorLeft + box.TransformX;
                    }
                    else if (transformBehaviorX == TransformBehavior.AnchorMaxOffset) {
                        localPosition.x = box.AnchorRight + box.TransformX;
                    }
                    else {
                        localPosition.x = box.TransformX;
                    }

                    if (transformBehaviorY == TransformBehavior.AnchorMinOffset) {
                        localPosition.y = box.AnchorTop + box.TransformY;
                    }
                    else if (transformBehaviorY == TransformBehavior.AnchorMaxOffset) {
                        localPosition.y = box.AnchorBottom + box.TransformY;
                    }
                    else {
                        localPosition.y = box.TransformY;
                    }

                    break;

                case LayoutBehavior.Normal:
                    if (transformBehaviorX == TransformBehavior.LayoutOffset) {
                        localPosition.x = box.localX + box.TransformX;
                    }
                    else {
                        localPosition.x = box.localX;
                    }

                    if (transformBehaviorY == TransformBehavior.LayoutOffset) {
                        localPosition.y = box.localY + box.TransformY;
                    }
                    else {
                        localPosition.y = box.localY;
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
//
                LayoutResult scrollbarResult = scrollbar.layoutResult;
                LayoutResult targetResult = scrollbar.targetElement.layoutResult;
//
                Rect trackRect = scrollbar.trackRect;
                scrollbarResult.layer = targetResult.layer + 1;
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

        private void HandleStylePropertyChanged(UIElement element, StyleProperty property) {
            // todo early-out if we haven't had a layout pass for the element yet
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) {
                return;
            }

            switch (property.propertyId) {
                case StylePropertyId.LayoutBehavior:
                    box.markedForLayout = true;
                    break;
                case StylePropertyId.LayoutType:
                    HandleLayoutChanged(element);
                    break;
            }

            if (box.IsInitialized) {
                box.OnStylePropertyChanged(property);
            }

            if (box.parent != null && box.parent.IsInitialized && (box.style.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                box.parent.OnChildStylePropertyChanged(box, property);
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
                parent.ReplaceChild(box, replace);
                // release(box)
                m_LayoutBoxMap[element.id] = replace;
            }

            // if removed -> release box
            // update map to hold new box
        }

        public void OnElementEnabled(UIElement element) {
            LayoutBox child = m_LayoutBoxMap.GetOrDefault(element.id);
            if (child == null) return;
            m_LayoutBoxMap.GetOrDefault(element.parent.id)?.OnChildEnabled(child);
        }

        public void OnElementDisabled(UIElement element) {
            LayoutBox child = m_LayoutBoxMap.GetOrDefault(element.id);
            if (child == null) return;
            m_LayoutBoxMap.GetOrDefault(element.parent.id)?.OnChildDisabled(child);
        }

        public void OnElementDestroyed(UIElement element) { }

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
                LayoutBox ptr = null;
                UIElement e = element.parent;
                while (ptr == null) {
                    e = e.parent;
                    ptr = m_LayoutBoxMap.GetOrDefault(e.id);
                }

                layoutBox.SetParent(ptr);
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

                for (int i = 0; i < parentElement.children.Length; i++) {
                    UIElement child = parentElement.children[i];
                    if ((child.flags & UIElementFlags.RequiresLayout) == 0) {
                        // don't create a layout box but do process the children
                        stack.Push(ValueTuple.Create(child, (LayoutBox) null));
                    }
                    else {
                        LayoutBox childBox = CreateLayoutBox(child);
                        childBox.SetParent(parentBox);
                        m_LayoutBoxMap.Add(child.id, childBox);
                        stack.Push(ValueTuple.Create(child, childBox));
                        m_Elements.Add(child);
                        m_PendingInitialization.Add(childBox);
                    }
                }
            }

            StackPool<ValueTuple<UIElement, LayoutBox>>.Release(stack);
        }

        public List<UIElement> QueryPoint(Vector2 point, List<UIElement> retn) {
            // todo if point is same as last point or point is off screen, do no work
            if (retn == null) {
                retn = ListPool<UIElement>.Get();
            }

            for (int i = 0; i < m_Elements.Count; i++) {
                LayoutResult layoutResult = m_Elements[i].layoutResult;
                UIElement element = m_Elements[i];               
               
                if (!layoutResult.ScreenRect.Contains(point)) {
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
                    retn.Add(m_Elements[i]);
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

        private static int ResolveRenderLayer(UIElement element) {
            RenderLayer layer = element.style.RenderLayer;
            switch (layer) {
                case RenderLayer.Unset:
                case RenderLayer.Default:
                    return element.depth;

                case RenderLayer.Parent:
                    return element.depth - 1;

                case RenderLayer.Template:
                    throw new NotImplementedException(); // need to save template root element somehow

                case RenderLayer.Modal:
                    return 100000;

                case RenderLayer.View:
                    return 500000;

                case RenderLayer.Screen:
                    return 1000000;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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