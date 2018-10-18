using System;
using System.Collections.Generic;
using Rendering;
using Src.Elements;
using Src.Extensions;
using Src.Layout;
using Src.Layout.LayoutTypes;
using Src.Util;
using UnityEngine;

namespace Src.Systems {

    public class LayoutSystem : ILayoutSystem {

        public event Action<VirtualScrollbar> onCreateVirtualScrollbar;
        public event Action<VirtualScrollbar> onDestroyVirtualScrollbar;

        protected readonly RootLayoutBox m_Root;

        // todo -- use arrays instead
        protected readonly List<LayoutBox> m_PendingInitialization;
        protected readonly Dictionary<int, LayoutBox> m_LayoutBoxMap;
        protected readonly IStyleSystem m_StyleSystem;

        protected readonly List<VirtualElement> m_VirtualElements;

        // todo -- convert to array / list w/ flag
        protected readonly HashSet<LayoutBox> m_PendingRectUpdates;

        private readonly List<UIElement> m_Elements;
        private bool m_RootRequiresLayout;

        private Size screenSize;

        public LayoutSystem(IStyleSystem styleSystem) {
            this.m_Root = new RootLayoutBox(this);
            this.m_StyleSystem = styleSystem;
            this.m_LayoutBoxMap = new Dictionary<int, LayoutBox>();
            this.m_PendingRectUpdates = new HashSet<LayoutBox>();
            this.m_Elements = new List<UIElement>();
            this.m_PendingInitialization = new List<LayoutBox>();
            this.m_VirtualElements = new List<VirtualElement>();
            this.m_RootRequiresLayout = true;
        }

        public Rect ViewportRect { get; private set; }

        public void OnReset() { }

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

            if (m_RootRequiresLayout) {
                m_RootRequiresLayout = false;
                m_Root.RunLayout();
            }

            RunLayout();
        }

        private void RunLayout() {

            bool forceLayout = false;
            Size screen = new Size(Screen.width, Screen.height);
            if (screenSize != screen) {
                screenSize = screen;
                forceLayout = true;
            }
            
            Stack<UIElement> stack = StackPool<UIElement>.Get();

            // todo -- use the skip tree! use it!
            // if we don't allow reparenting, could just use a flat sorted list
            // as long as the parent is laid out before the child that should be fine

            UIElement element = m_Root.children[0].element;
            LayoutResult layoutResult = element.layoutResult;

            stack.Push(element);

            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);

            if (forceLayout || box.markedForLayout) {
                box.RunLayout();
                box.markedForLayout = false;
            }

            int depth = 0;
            int computedLayer = ResolveRenderLayer(element) - element.ComputedStyle.LayerOffset;
            int zIndex = element.ComputedStyle.ZIndex;

            if (depth > computedLayer) {
                zIndex -= 2000;
            }
            else if (depth < computedLayer) {
                zIndex += -2000;
            }

            if (box.IsIgnored) {
                box.allocatedWidth = box.actualWidth;
                box.allocatedHeight = box.actualHeight;
            }

            layoutResult.ActualSize = new Size(box.actualWidth, box.actualHeight);
            layoutResult.AllocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
            layoutResult.ContentOffset = new Vector2(box.ContentOffsetLeft, box.ContentOffsetTop);
            layoutResult.Scale = new Vector2(box.style.TransformScaleX, box.style.TransformScaleY);
            layoutResult.LocalPosition = ResolveLocalPosition(box);
            // todo -- wrong?
            layoutResult.ScreenPosition = ViewportRect.min + layoutResult.localPosition;
            layoutResult.Rotation = box.style.TransformRotation;
            layoutResult.Layer = computedLayer;
            layoutResult.ZIndex = zIndex;
            layoutResult.clipRect = ViewportRect;
            element.layoutResult = layoutResult;

            while (stack.Count > 0) {
                UIElement current = stack.Pop();

                if (!current.isEnabled) {
                    continue;
                }

                if (current.ownChildren == null) {
                    continue;
                }

                for (int i = 0; i < current.ownChildren.Length; i++) {
                    element = current.ownChildren[i];

                    box = m_LayoutBoxMap.GetOrDefault(element.id);

                    if (box == null) {
                        stack.Push(element);
                        continue;
                    }

                    if (forceLayout || box.markedForLayout) {
                        box.RunLayout();
                        box.markedForLayout = false;
                    }

                    depth = element.depth;
                    computedLayer = ResolveRenderLayer(element) - element.ComputedStyle.LayerOffset;
                    zIndex = element.ComputedStyle.ZIndex;

                    if (depth > computedLayer) {
                        zIndex -= 2000;
                    }
                    else if (depth < computedLayer) {
                        zIndex += -2000;
                    }

                    layoutResult = element.layoutResult;

                    if (box.IsIgnored) {
                        box.allocatedWidth = box.actualWidth;
                        box.allocatedHeight = box.actualHeight;
                    }

                    layoutResult.LocalPosition = ResolveLocalPosition(box);
                    layoutResult.ContentOffset = new Vector2(box.ContentOffsetLeft, box.ContentOffsetTop);
                    layoutResult.ActualSize = new Size(box.actualWidth, box.actualHeight);
                    layoutResult.AllocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
                    layoutResult.ScreenPosition = current.layoutResult.screenPosition + layoutResult.localPosition;
                    layoutResult.Scale = new Vector2(box.style.TransformScaleX, box.style.TransformScaleY);
                    layoutResult.Rotation = box.style.TransformRotation;
                    layoutResult.Layer = computedLayer;
                    layoutResult.ZIndex = zIndex;

                    Rect clipRect = ViewportRect;
                    UIElement ptr = element.parent;

                    // find ancestor where layer is higher
                    while (ptr != null && ptr.layoutResult.layer > computedLayer) {
                        ptr = ptr.parent;
                    }

                    // from this point on we are trying to find the clip rect to use

                    if (ptr != null) {
                        bool handlesHorizontal = ptr.style.HandlesOverflowX;
                        bool handlesVertical = ptr.style.HandlesOverflowY;
                        if (handlesHorizontal && handlesVertical) {
                            Rect r = new Rect(ptr.layoutResult.screenPosition, ptr.layoutResult.allocatedSize);
                            clipRect = RectIntersect(clipRect, RectIntersect(r, ptr.layoutResult.clipRect));
                        }
                        else if (handlesHorizontal) {
                            Rect r = new Rect(
                                ptr.layoutResult.screenPosition.x,
                                ptr.layoutResult.clipRect.y,
                                ptr.layoutResult.AllocatedWidth,
                                ptr.layoutResult.clipRect.height
                            );
                            clipRect = RectIntersect(r, clipRect);
                        }
                        else if (handlesVertical) {
                            Rect r = new Rect(
                                ptr.layoutResult.clipRect.x,
                                ptr.layoutResult.screenPosition.y,
                                ptr.layoutResult.clipRect.width,
                                ptr.layoutResult.AllocatedHeight
                            );
                            clipRect = RectIntersect(r, clipRect);
                        }
                        else {
                            clipRect = ptr.layoutResult.clipRect;
                        }
                    }

                    layoutResult.clipRect = clipRect;
                    element.layoutResult = layoutResult;
                    stack.Push(element);
                }
            }

            StackPool<UIElement>.Release(stack);
        }

        private Vector2 ResolveLocalPosition(LayoutBox box) {
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

        private void HandleScrollbars(LayoutBox box) {
            UIElement element = box.element;
            LayoutResult layoutResult = element.layoutResult;
            VirtualScrollbar vertical = box.verticalScrollbar;
            VirtualScrollbar horizontal = box.horizontalScrollbar;
            if (box.actualWidth <= box.allocatedWidth) {
                if (horizontal != null) {
                    onDestroyVirtualScrollbar?.Invoke(horizontal);
                    m_Elements.Remove(horizontal);
                    box.horizontalScrollbar = null; // todo -- pool
                }
            }
            else {
                if (horizontal == null) {
                    horizontal = new VirtualScrollbar(element, ScrollbarOrientation.Horizontal);
                    m_Elements.Add(horizontal);
//                        onCreateVirtualScrollbar?.Invoke(horizontal);
//                        Extents childExtents = GetLocalExtents(box.children);
//                        float offsetX = (childExtents.min.x < 0) ? -childExtents.min.x / box.allocatedWidth : 0f;
//                        element.scrollOffset = new Vector2(offsetX, element.scrollOffset.y);
                }

                Rect trackRect = horizontal.GetTrackRect();
                horizontal.layoutResult = new LayoutResult() {
                    screenPosition = new Vector2(trackRect.x, trackRect.y),
                    actualSize = new Size(layoutResult.AllocatedHeight, 5f),
                    allocatedSize = new Size(box.actualWidth, 5f), // todo -- wrong?
                };
            }

            if (box.actualHeight <= box.allocatedHeight) {
                if (vertical != null) {
                    onDestroyVirtualScrollbar?.Invoke(vertical);
                    m_Elements.Remove(vertical);
                    box.verticalScrollbar = null; // todo -- pool
                }
            }
            else {
                if (vertical == null) {
                    vertical = new VirtualScrollbar(element, ScrollbarOrientation.Vertical);
                    element.style.InitializeScrollbar(vertical);
                    LayoutBox scrollbarLayoutBox = new FixedLayoutBox(this, vertical);
                    LayoutBox scrollbarHandleLayoutBox = new FixedLayoutBox(this, vertical.handle);

                    scrollbarLayoutBox.parent = box;
                    scrollbarHandleLayoutBox.parent = scrollbarLayoutBox;

                    scrollbarLayoutBox.IsInitialized = true;
                    scrollbarHandleLayoutBox.IsInitialized = true;

                    m_LayoutBoxMap.Add(vertical.id, scrollbarLayoutBox);
                    m_LayoutBoxMap.Add(vertical.handle.id, scrollbarHandleLayoutBox);

                    m_Elements.Add(vertical);
                    m_Elements.Add(vertical.handle);

                    m_VirtualElements.Add(vertical);
                    m_VirtualElements.Add(vertical.handle);

                    Extents childExtents = GetLocalExtents(box.children);
                    float offsetY = (childExtents.min.y < 0) ? -childExtents.min.y / box.allocatedHeight : 0f;
                    element.scrollOffset = new Vector2(element.scrollOffset.x, offsetY);

                    scrollbarLayoutBox.allocatedWidth = vertical.trackSize;
                    scrollbarLayoutBox.allocatedHeight = element.layoutResult.AllocatedHeight;

                    scrollbarHandleLayoutBox.allocatedWidth = vertical.handleSize;
                    scrollbarHandleLayoutBox.allocatedHeight = element.layoutResult.AllocatedHeight;

                    Rect trackRect = vertical.GetTrackRect();
                    LayoutResult result = new LayoutResult();
                    result.actualSize = new Size(vertical.trackSize, element.layoutResult.AllocatedHeight);
                    result.allocatedSize = new Size(vertical.trackSize, element.layoutResult.AllocatedHeight);
                    result.localPosition = new Vector2(trackRect.x, trackRect.y);
                    vertical.layoutResult = result;

                    Rect handleRect = vertical.HandleRect;
                    Vector2 handlePosition = vertical.handlePosition;
                    result = new LayoutResult();
                    result.localPosition = new Vector2(0, handlePosition.y);
                    result.actualSize = new Size(handleRect.width, handleRect.height);
                    result.allocatedSize = new Size(handleRect.width, handleRect.height);
                    vertical.handle.layoutResult = result;

                    //  m_PendingRectUpdates.Add(scrollbarLayoutBox);
                    //   m_PendingRectUpdates.Add(scrollbarHandleLayoutBox);

                    onCreateVirtualScrollbar?.Invoke(vertical);
                }

//                    else {
//                        Rect trackRect = vertical.GetTrackRect();
//                        vertical.layoutResult = new LayoutResult() {
//                            screenPosition = new Vector2(trackRect.x, trackRect.y),
//                            actualSize = new Size(layoutResult.allocatedHeight, 5f),
//                            allocatedSize = new Size(box.actualHeight, 5f),
//                        };
//                    }
            }
        }

        private static Rect RectIntersect(Rect a, Rect b) {
            float xMin = a.x > b.x ? a.x : b.x;
            float xMax = a.x + a.width < b.x + b.width ? a.x + a.width : b.x + b.width;
            float yMin = a.y > b.y ? a.y : b.y;
            float yMax = a.y + a.height < b.y + b.height ? a.y + a.height : b.y + b.height;

            if (xMax >= xMin && yMax >= yMin) {
                return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            }

            return new Rect(0f, 0f, 0f, 0f);
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

        public void OnReady() {
            m_StyleSystem.onTextContentChanged += HandleTextContentChanged;
            m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        public void OnInitialize() { }

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

        private void HandleScrollBehaviorChanged(LayoutBox box) {
//            switch (box.style.ScrollBehavior) {
//                case ScrollBehavior.Normal:
//                case ScrollBehavior.Fixed:
//                case ScrollBehavior.Sticky:
//                    break;
//
//            }
        }

        // todo -- remove, let textbox handle this
        private void HandleTextContentChanged(UIElement element, string content) {
            LayoutBox box;
            if (m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                TextLayoutBox textBox = box as TextLayoutBox;
                textBox?.OnTextContentUpdated();
            }
        }

        private void HandleLayoutChanged(UIElement element) {
            LayoutBox box;
            if (!m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                return;
            }

            LayoutBox parent = box.parent;
            LayoutBox replace = box;

            switch (element.style.computedStyle.LayoutType) {
                case LayoutType.Radial:
                    if (!(box is RadialLayoutBox)) {
                        replace = new RadialLayoutBox(this, element);
                    }

                    break;
                case LayoutType.Fixed:
                    if (!(box is FixedLayoutBox)) {
                        replace = new FixedLayoutBox(this, element);
                    }

                    break;
                case LayoutType.Flex:
                    if (!(box is FlexLayoutBox)) {
                        replace = new FlexLayoutBox(this, element);
                    }

                    break;
                case LayoutType.Grid:
                    if (!(box is GridLayoutBox)) {
                        replace = new GridLayoutBox(this, element);
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

        public void OnElementCreated(UIElement element) { }

        public void OnElementMoved(UIElement element, int newIndex, int oldIndex) { }

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

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        // todo pool boxes
        private LayoutBox CreateLayoutBox(UIElement element) {
            if ((element is UITextElement)) {
                return new TextLayoutBox(this, element);
            }

            if ((element is UIImageElement)) {
                return new ImageLayoutBox(this, element);
            }

            switch (element.style.computedStyle.LayoutType) {
                case LayoutType.Flex:
                    return new FlexLayoutBox(this, element);

                case LayoutType.Fixed:
                    return new FixedLayoutBox(this, element);

                case LayoutType.Grid:
                    return new GridLayoutBox(this, element);

                case LayoutType.Radial:
                    return new RadialLayoutBox(this, element);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnElementCreatedFromTemplate(MetaData elementData) {
            LayoutBox layoutBox = CreateLayoutBox(elementData.element);
            Stack<ValueTuple<MetaData, LayoutBox>> stack = StackPool<ValueTuple<MetaData, LayoutBox>>.Get();

            LayoutBox parent = m_Root;
            if (elementData.element.parent != null) {
                LayoutBox ptr = null;
                UIElement e = elementData.element.parent;
                while (ptr == null) {
                    e = e.parent;
                    ptr = m_LayoutBoxMap.GetOrDefault(e.id);
                }

                parent = ptr;
            }

            layoutBox.SetParent(parent);
            m_LayoutBoxMap.Add(elementData.element.id, layoutBox);
            stack.Push(ValueTuple.Create(elementData, layoutBox));

            m_Elements.Add(elementData.element);
            m_PendingInitialization.Add(layoutBox);

            while (stack.Count > 0) {
                ValueTuple<MetaData, LayoutBox> item = stack.Pop();
                MetaData parentData = item.Item1;
                LayoutBox parentBox = item.Item2;

                if (parentBox == null) {
                    LayoutBox ptr = null;
                    UIElement e = parentData.element;
                    while (ptr == null) {
                        e = e.parent;
                        ptr = m_LayoutBoxMap.GetOrDefault(e.id);
                    }

                    parentBox = ptr;
                }

                for (int i = 0; i < parentData.children.Count; i++) {
                    MetaData childData = parentData.children[i];
                    if ((childData.element.flags & UIElementFlags.RequiresLayout) == 0) {
                        // don't create a layout box but do process the children
                        stack.Push(ValueTuple.Create(childData, (LayoutBox) null));
                    }
                    else {
                        LayoutBox childBox = CreateLayoutBox(childData.element);
                        childBox.SetParent(parentBox);
                        m_LayoutBoxMap.Add(childData.element.id, childBox);
                        stack.Push(ValueTuple.Create(childData, childBox));
                        m_Elements.Add(childData.element);
                        m_PendingInitialization.Add(childBox);
                    }
                }
            }

            StackPool<ValueTuple<MetaData, LayoutBox>>.Release(stack);
        }

        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) { }

        public void SetViewportRect(Rect viewportRect) {
            if (ViewportRect != viewportRect) {
                ViewportRect = viewportRect;
                m_Root.allocatedWidth = viewportRect.width;
                m_Root.allocatedHeight = viewportRect.height;
                m_Root.actualWidth = viewportRect.width;
                m_Root.actualHeight = viewportRect.height;
                m_RootRequiresLayout = true;

                // todo -- tell all layout boxes / run full layout
            }
        }

        public List<UIElement> QueryPoint(Vector2 point, List<UIElement> retn) {
            // todo if point is same as last point or point is off screen, do no work
            if (retn == null) {
                retn = ListPool<UIElement>.Get();
            }

            for (int i = 0; i < m_Elements.Count; i++) {
                LayoutResult layoutResult = m_Elements[i].layoutResult;
                UIElement element = m_Elements[i];

                // todo -- replace w/ quad tree
                if (!layoutResult.ScreenRect.Contains(point)) {
                    continue;
                }

                UIElement ptr = element.parent;
                while (ptr != null) {
                    Vector2 screenPosition = ptr.layoutResult.screenPosition;
                    if (ptr.style.HandlesOverflowX) {
                        if (point.x < screenPosition.x || point.x > screenPosition.x + ptr.layoutResult.AllocatedWidth) {
                            break;
                        }
                    }

                    if (ptr.style.HandlesOverflowY) {
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

        private int ResolveRenderLayer(UIElement element) {
            RenderLayer layer = element.style.computedStyle.RenderLayer;
            switch (layer) {
                case RenderLayer.Unset:
                case RenderLayer.Default:
                    return element.depth;

                case RenderLayer.Parent:
                    return element.depth - 1;

                case RenderLayer.Template:
                    return element.templateParent.depth;

                case RenderLayer.Modal:
                    return -1000;

                case RenderLayer.View:
                    return -500;

                case RenderLayer.Screen:
                    return -2000;

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