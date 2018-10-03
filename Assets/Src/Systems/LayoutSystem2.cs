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

    public class LayoutSystem2 : ILayoutSystem {

        public event Action<VirtualScrollbar> onCreateVirtualScrollbar;
        public event Action<VirtualScrollbar> onDestroyVirtualScrollbar;

        protected readonly RootLayoutBox root;
        protected readonly List<LayoutBox> m_UpdateRequiredElements;
        protected readonly List<LayoutBox> m_RectUpdates;
        protected readonly Dictionary<int, LayoutBox> m_LayoutBoxMap;
        protected readonly IStyleSystem m_StyleSystem;

        private readonly List<UIElement> m_Elements;

        public LayoutSystem2(ITextSizeCalculator textSizeCalculator, IStyleSystem styleSystem) {
            this.root = new RootLayoutBox(this);
            this.m_StyleSystem = styleSystem;
            this.m_LayoutBoxMap = new Dictionary<int, LayoutBox>();
            this.m_UpdateRequiredElements = new List<LayoutBox>();
            this.m_RectUpdates = new List<LayoutBox>();
            this.m_Elements = new List<UIElement>();
        }

        public Rect ViewportRect { get; private set; }

        public void OnReset() { }

        public void OnUpdate() {

            if (m_UpdateRequiredElements.Count == 0) return;

            m_UpdateRequiredElements.Sort((a, b) => a.element.depth > b.element.depth ? 1 : -1);

            for (int i = 0; i < m_UpdateRequiredElements.Count; i++) {
                m_UpdateRequiredElements[i].RunLayout();
            }

            m_RectUpdates.Sort((a, b) => a.element.depth > b.element.depth ? 1 : -1);

            for (int i = 0; i < m_RectUpdates.Count; i++) {
             
                LayoutBox box = m_RectUpdates[i];
                UIElement element = box.element;
                Vector2 localPosition = new Vector2(box.localX, box.localY);
                LayoutResult layoutResult = new LayoutResult(element);
                layoutResult.localPosition = localPosition;
                layoutResult.screenPosition = localPosition + box.parent?.element?.layoutResult.screenPosition ?? Vector2.zero;
                layoutResult.size = new Size(box.actualWidth, box.actualHeight);
                layoutResult.allocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
                
                element.layoutResult = layoutResult;

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
                        horizontal = new VirtualScrollbar(element, ScrollbarOrientation.Vertical);
                        m_Elements.Add(horizontal);
                        onCreateVirtualScrollbar?.Invoke(horizontal);
                        Extents childExtents = GetLocalExtents(box.children);
                        float offsetX = (childExtents.min.x < 0) ? -childExtents.min.x / box.allocatedWidth : 0f;
                        element.scrollOffset = new Vector2(offsetX, element.scrollOffset.y);
                    }
                    Rect trackRect = horizontal.GetTrackRect();
                    horizontal.layoutResult = new LayoutResult(horizontal) {
                        screenPosition = new Vector2(trackRect.x, trackRect.y),
                        size = new Size(layoutResult.allocatedHeight, 5f),
                        contentSize = new Size(box.actualWidth, 5f),
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
                        m_Elements.Add(vertical);
                        onCreateVirtualScrollbar?.Invoke(vertical);
                        Extents childExtents = GetLocalExtents(box.children);
                        float offsetY = (childExtents.min.y < 0) ? -childExtents.min.y / box.allocatedHeight : 0f;
                        element.scrollOffset = new Vector2(element.scrollOffset.x, offsetY);
                    }
                    Rect trackRect = vertical.GetTrackRect();
                    vertical.layoutResult = new LayoutResult(vertical) {
                        screenPosition = new Vector2(trackRect.x, trackRect.y),
                        size = new Size(layoutResult.allocatedHeight, 5f),
                        contentSize = new Size(box.actualHeight, 5f),
                    };
                }
            }

            m_UpdateRequiredElements.Clear();
            m_RectUpdates.Clear();
        }

        internal void OnRectChanged(LayoutBox layoutBox) {
            if (!m_RectUpdates.Contains(layoutBox)) {
                m_RectUpdates.Add(layoutBox);
            }
        }

        internal void OnLayoutBoxOverflow(LayoutBox box) {
            if (box.element.style.HandlesOverflowX) {
                
            }
        }

        internal void OnLayoutBoxUnderflow(LayoutBox box) { }

        internal void PositionChanged(LayoutBox layoutBox) {
            m_RectUpdates.Add(layoutBox);
        }

        internal void RequestLayout(LayoutBox layoutBox) {
            // todo replace w/ set
            if (layoutBox == root) return;
            if (!m_UpdateRequiredElements.Contains(layoutBox)) {
                m_UpdateRequiredElements.Add(layoutBox);
            }
        }

        public void OnDestroy() { }

        public void OnReady() {
            m_StyleSystem.onBorderChanged += HandleContentBoxChanged;
            m_StyleSystem.onPaddingChanged += HandleContentBoxChanged;
            m_StyleSystem.onMarginChanged += HandleContentBoxChanged;
            m_StyleSystem.onLayoutChanged += HandleLayoutChanged;
            m_StyleSystem.onRectChanged += HandleRectChanged;
            m_StyleSystem.onTextContentChanged += HandleTextContentChanged;
            m_StyleSystem.onMinWidthChanged += HandleSizeConstraintChanged;
            m_StyleSystem.onMaxWidthChanged += HandleSizeConstraintChanged;
            m_StyleSystem.onPreferredWidthChanged += HandleSizeChanged;
            m_StyleSystem.onPreferredHeightChanged += HandleSizeChanged;
        }

        public void OnInitialize() {
//            m_StyleSystem.onOverflowPropertyChanged += HandleOverflowChanged;

            //m_StyleSystem.onLayoutDirectionChanged += HandleLayoutDirectionChanged;
            //m_StyleSystem.onLayoutWrapChanged += HandleWrapStateChanged;
            //m_StyleSystem.onFlowStateChanged == HandleFlowStateChanged;
        }

        private void HandleSizeChanged(UIElement element, UIMeasurement arg2, UIMeasurement arg3) {
            if (element.parent == null) return;
            m_LayoutBoxMap.GetOrDefault(element.parent.id)?.OnChildSizeChanged();
        }

        private void HandleSizeConstraintChanged(UIElement element, UIMeasurement newMinWidth, UIMeasurement oldMinWidth) {
            if (element.parent == null) return;
            m_LayoutBoxMap.GetOrDefault(element.id)?.OnSizeConstraintChanged(); // MarkForLayout(UpdateType.Constraint)
        }

        private void HandleFontPropertyChanged(UIElement element, TextStyle textStyle) { }

        // todo -- eventually we can have this take a patch instead of a whole new string
        // that way we don't have to re-layout and re-measure parts of the string that didn't change
        private void HandleTextContentChanged(UIElement element, string content) {
            LayoutBox box;
            if (m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                TextContainerLayoutBox textBox = box.parent as TextContainerLayoutBox;

                textBox?.SetTextContent(content);
            }
        }

        private void HandleRectChanged(UIElement element, Dimensions d) {
            // if min changes and current >= new min  no layout needed
            // if max changes and current <= new max  no layout needed

            m_LayoutBoxMap.GetOrDefault(element.id)?.OnContentRectChanged();
        }

        private void HandleFlexLayoutStateChanged() { }

        private void HandleGridLayoutStateChanged() { }

        private void HandleLayoutChanged(UIElement element, LayoutParameters layoutParameters) {
            LayoutBox box;
            if (!m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                return;
            }

            LayoutBox parent = box.parent;
            LayoutBox replace = box;

            switch (layoutParameters.type) {
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

        private void HandleContentBoxChanged(UIElement element, ContentBoxRect contentBox) {
            LayoutBox box;
            if (m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                box.OnContentRectChanged();
            }
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

        private LayoutBox CreateLayoutBox(UIElement element) {
            if ((element is UITextContainerElement)) {
                return new TextContainerLayoutBox(this, element);
            }

            switch (element.style.computedStyle.LayoutType) {
                case LayoutType.Flex:
                    return new FlexLayoutBox(this, element);
                case LayoutType.Fixed:
                    return new FixedLayoutBox(this, element);
                case LayoutType.Grid:
                    return new GridLayoutBox(this, element);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnElementCreatedFromTemplate(MetaData elementData) {
            // todo pool
            LayoutBox layoutBox = CreateLayoutBox(elementData.element);
            Stack<ValueTuple<MetaData, LayoutBox>> stack = StackPool<ValueTuple<MetaData, LayoutBox>>.Get();

            LayoutBox parent = root;
            if (elementData.element.parent != null) {
                if (!m_LayoutBoxMap.TryGetValue(elementData.element.parent.id, out parent)) {
                    parent = root;
                }
            }

            layoutBox.SetParent(parent);
            m_LayoutBoxMap.Add(elementData.element.id, layoutBox);
            stack.Push(ValueTuple.Create(elementData, layoutBox));

            m_UpdateRequiredElements.Add(layoutBox);
            m_Elements.Add(elementData.element);

            while (stack.Count > 0) {
                ValueTuple<MetaData, LayoutBox> item = stack.Pop();
                MetaData parentData = item.Item1;
                LayoutBox parentBox = item.Item2;

                for (int i = 0; i < parentData.children.Count; i++) {
                    MetaData childData = parentData.children[i];
                    LayoutBox childBox = CreateLayoutBox(childData.element);
                    childBox.SetParent(parentBox);
                    m_LayoutBoxMap.Add(childData.element.id, childBox);
                    stack.Push(ValueTuple.Create(childData, childBox));
                    m_Elements.Add(childData.element);
                }
            }

            StackPool<ValueTuple<MetaData, LayoutBox>>.Release(stack);
        }

        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) { }

        public void SetViewportRect(Rect viewportRect) {
            ViewportRect = viewportRect;
            root.allocatedWidth = viewportRect.width;
            root.allocatedHeight = viewportRect.height;
            root.actualWidth = viewportRect.width;
            root.actualHeight = viewportRect.height;
            root.RunLayout();
        }

        public List<UIElement> QueryPoint(Vector2 point, List<UIElement> retn) {
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
                        if (point.x < screenPosition.x || point.x > screenPosition.x + ptr.layoutResult.allocatedWidth) {
                            break;
                        }
                    }

                    if (ptr.style.HandlesOverflowY) {
                        if (point.y < screenPosition.y || point.y > screenPosition.y + ptr.layoutResult.allocatedHeight) {
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

        public List<LayoutResult> GetLayoutResults(List<LayoutResult> retn) {
            return new List<LayoutResult>();
        }
        private static Extents GetLocalExtents(List<LayoutBox> children) {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                
                if(child.element.isDisabled) continue;
                
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