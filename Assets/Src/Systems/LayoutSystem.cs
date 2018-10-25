using System;
using System.Collections.Generic;
using Rendering;
using Src.Elements;
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
        protected readonly IntMap<LayoutBox> m_LayoutBoxMap;
        protected readonly IStyleSystem m_StyleSystem;
        protected readonly IntMap<UIElement[]> m_QueryGrid;
        protected readonly List<VirtualElement> m_VirtualElements;

        private readonly List<UIElement> m_Elements;
        private bool m_RootRequiresLayout;
        private Size m_ScreenSize;

        public LayoutSystem(IStyleSystem styleSystem) {
            this.m_Root = new RootLayoutBox(this);
            this.m_StyleSystem = styleSystem;
            this.m_LayoutBoxMap = new IntMap<LayoutBox>();
            this.m_Elements = new List<UIElement>();
            this.m_PendingInitialization = new List<LayoutBox>();
            this.m_VirtualElements = new List<VirtualElement>();
            this.m_QueryGrid = new IntMap<UIElement[]>();
            this.m_RootRequiresLayout = true;
        }

        public Rect ViewportRect { get; private set; }

        public void OnReset() {
            m_LayoutBoxMap.Clear();
            m_Elements.Clear();
            m_PendingInitialization.Clear();
            m_VirtualElements.Clear();
            m_QueryGrid.Clear();
            m_RootRequiresLayout = true;
            m_Root.children.Clear();
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

            if (m_RootRequiresLayout) {
                m_RootRequiresLayout = false;
                m_Root.RunLayout();
            }

            RunLayout();
        }

        private void RunLayout() {
            bool forceLayout = false;
            Size screen = new Size(Screen.width, Screen.height);
            if (m_ScreenSize != screen) {
                m_ScreenSize = screen;
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
            layoutResult.ScreenPosition = layoutResult.localPosition;
            layoutResult.Rotation = box.style.TransformRotation;
            layoutResult.Layer = computedLayer;
            layoutResult.ZIndex = zIndex;
            layoutResult.clipRect = new Rect(0, 0, ViewportRect.width, ViewportRect.height);
            element.layoutResult = layoutResult;

            CreateOrDestroyScrollbars(box);

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
                    Rect oldScreenRect = layoutResult.ScreenRect;

                    if (box.IsIgnored) {
                        box.allocatedWidth = box.actualWidth;
                        box.allocatedHeight = box.actualHeight;
                    }

                    LayoutBox parentBox = box.parent;
                    Vector2 scrollOffset = new Vector2();
                    scrollOffset.x = (parentBox.actualWidth - parentBox.allocatedWidth) * parentBox.element.scrollOffset.x;
                    scrollOffset.y = (parentBox.actualHeight - parentBox.allocatedHeight) * parentBox.element.scrollOffset.y;
                    
                    layoutResult.LocalPosition = ResolveLocalPosition(box) - scrollOffset;
                    layoutResult.ContentOffset = new Vector2(box.ContentOffsetLeft, box.ContentOffsetTop);
                    layoutResult.ActualSize = new Size(box.actualWidth, box.actualHeight);
                    layoutResult.AllocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
                    layoutResult.ScreenPosition = current.layoutResult.screenPosition + layoutResult.localPosition;
                    layoutResult.Scale = new Vector2(box.style.TransformScaleX, box.style.TransformScaleY);
                    layoutResult.Rotation = box.style.TransformRotation;
                    layoutResult.Layer = computedLayer;
                    layoutResult.ZIndex = zIndex;

                    Rect clipRect = new Rect(0, 0, ViewportRect.width, ViewportRect.height);
                    UIElement ptr = element.parent;

                    // find ancestor where layer is higher, might not be our parent
                    while (ptr != null && ptr.layoutResult.layer > computedLayer) {
                        ptr = ptr.parent;
                    }

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
                if (vertical == null) {
                    vertical = new VirtualScrollbar(element, ScrollbarOrientation.Vertical);
                    element.style.InitializeScrollbar(vertical);
                    // todo -- depth index needs to be set
                    
                    m_Elements.Add(vertical);
                    m_Elements.Add(vertical.handle);

                    m_VirtualElements.Add(vertical);

                    Extents childExtents = GetLocalExtents(box.children);
                    float offsetY = (childExtents.min.y < 0) ? -childExtents.min.y / box.allocatedHeight : 0f;
                    element.scrollOffset = new Vector2(element.scrollOffset.x, offsetY);

                    onCreateVirtualScrollbar?.Invoke(vertical);
                    box.verticalScrollbar = vertical;
                }
            }
        }

        private void UpdateScrollbarLayouts() {
            for (int i = 0; i < m_VirtualElements.Count; i++) {
                VirtualScrollbar scrollbar = (VirtualScrollbar) m_VirtualElements[i];
                VirtualScrollbarHandle handle = scrollbar.handle;

                LayoutResult scrollbarResult = scrollbar.layoutResult;
                LayoutResult handleResult = handle.layoutResult;
                LayoutResult targetResult = scrollbar.targetElement.layoutResult;

                Rect trackRect = scrollbar.TrackRect;
                scrollbarResult.layer = targetResult.layer + 1;
                scrollbarResult.localPosition = new Vector2(trackRect.x, trackRect.y);
                scrollbarResult.screenPosition = targetResult.screenPosition + scrollbarResult.localPosition;
                scrollbarResult.clipRect = targetResult.clipRect;
                scrollbarResult.actualSize = new Size(trackRect.width, trackRect.height);
                scrollbarResult.allocatedSize = scrollbarResult.actualSize;

                Rect handleRect = scrollbar.HandleRect;
                handleResult.layer = scrollbarResult.layer + 1;
                handleResult.localPosition = new Vector2(handleRect.x, handleRect.y);
                handleResult.screenPosition = scrollbarResult.screenPosition + handleResult.localPosition;
                handleResult.clipRect = targetResult.clipRect;
                handleResult.actualSize = new Size(handleRect.width, handleRect.height);
                handleResult.allocatedSize = handleResult.actualSize;

                scrollbar.layoutResult = scrollbarResult;
                handle.layoutResult = handleResult;
            }
        }

        private static Rect RectIntersect(Rect a, Rect b) {
            float xMin = a.x > b.x ? a.x : b.x;
            float xMax = a.x + a.width < b.x + b.width ? a.x + a.width : b.x + b.width;
            float yMin = a.y > b.y ? a.y : b.y;
            float yMax = a.y + a.height < b.y + b.height ? a.y + a.height : b.y + b.height;

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
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

        private static int ResolveRenderLayer(UIElement element) {
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