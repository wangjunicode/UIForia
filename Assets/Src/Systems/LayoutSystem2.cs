using System;
using System.Collections.Generic;
using Rendering;
using Src.Elements;
using Src.Extensions;
using Src.Layout.LayoutTypes;
using Src.Util;
using UnityEngine;

namespace Src.Systems {

    public class LayoutSystem2 : ILayoutSystem {

        public event Action<VirtualScrollbar> onCreateVirtualScrollbar;
        public event Action<VirtualScrollbar> onDestroyVirtualScrollbar;

        protected readonly RootLayoutBox root;
        protected readonly List<LayoutBox> m_RectUpdates;
        protected readonly List<LayoutBox> m_PendingWidthLayoutUpdates;
        protected readonly List<LayoutBox> m_PendingHeightLayoutUpdates;
        protected readonly HashSet<LayoutBox> m_PendingRectUpdates;
        protected readonly Dictionary<int, LayoutBox> m_LayoutBoxMap;
        protected readonly IStyleSystem m_StyleSystem;

        private readonly List<UIElement> m_Elements;

        public LayoutSystem2(IStyleSystem styleSystem) {
            this.root = new RootLayoutBox(this);
            this.m_StyleSystem = styleSystem;
            this.m_LayoutBoxMap = new Dictionary<int, LayoutBox>();
            this.m_PendingWidthLayoutUpdates = new List<LayoutBox>();
            this.m_PendingHeightLayoutUpdates = new List<LayoutBox>();
            this.m_PendingRectUpdates = new HashSet<LayoutBox>();
            this.m_RectUpdates = new List<LayoutBox>();
            this.m_Elements = new List<UIElement>();
        }

        public Rect ViewportRect { get; private set; }

        public void OnReset() { }

        public void OnUpdate() {
            m_RectUpdates.Clear();

            if (m_PendingWidthLayoutUpdates.Count == 0 && m_PendingHeightLayoutUpdates.Count == 0 && m_PendingRectUpdates.Count == 0) {
                return;
            }

            m_PendingWidthLayoutUpdates.Sort((a, b) => a.element.depth > b.element.depth ? 1 : -1);

            for (int i = 0; i < m_PendingWidthLayoutUpdates.Count; i++) {
                m_PendingWidthLayoutUpdates[i].RunWidthLayout();
                m_PendingRectUpdates.Add(m_PendingWidthLayoutUpdates[i]);
            }
            
            m_PendingHeightLayoutUpdates.Sort((a, b) => a.element.depth > b.element.depth ? 1 : -1);

            for (int i = 0; i < m_PendingHeightLayoutUpdates.Count; i++) {
                m_PendingHeightLayoutUpdates[i].RunHeightLayout();
                m_PendingRectUpdates.Add(m_PendingHeightLayoutUpdates[i]);
            }
            
            foreach (LayoutBox layoutBox in m_PendingRectUpdates) {
                m_RectUpdates.Add(layoutBox);
            }
            
            for (int i = 0; i < m_PendingWidthLayoutUpdates.Count; i++) {
                m_PendingWidthLayoutUpdates[i].markedForWidthLayout = false;
            }
            
            for (int i = 0; i < m_PendingHeightLayoutUpdates.Count; i++) {
                m_PendingHeightLayoutUpdates[i].markedForHeightLayout = false;
            }
            
            m_PendingWidthLayoutUpdates.Clear();
            m_PendingHeightLayoutUpdates.Clear();
            m_PendingRectUpdates.Clear();

            m_RectUpdates.Sort((a, b) => a.element.depth > b.element.depth ? 1 : -1);

            for (int i = 0; i < m_RectUpdates.Count; i++) {
                LayoutBox box = m_RectUpdates[i];
                UIElement element = box.element;
                Vector2 localPosition = new Vector2(box.localX, box.localY);

                LayoutResult layoutResult = new LayoutResult();
                layoutResult.localPosition = localPosition;
                layoutResult.screenPosition = localPosition + box.parent?.element?.layoutResult.screenPosition ?? Vector2.zero;
                layoutResult.contentOffset = new Vector2(box.PaddingLeft + box.BorderLeft, box.PaddingTop + box.BorderTop);
                layoutResult.actualSize = new Size(box.actualWidth, box.actualHeight);
                layoutResult.allocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
                
                float contentWidth = box.allocatedWidth - (box.PaddingHorizontal + box.BorderHorizontal);
                float contentHeight = box.allocatedHeight - (box.PaddingVertical + box.BorderVertical);
                layoutResult.contentSize = new Size(contentWidth, contentHeight);
                element.layoutResult = layoutResult;

                // todo -- possible 2nd phase for 'attach' layouts ie fixed / sticky / anchor to edge, this would only do positioning and not sizing

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
                    horizontal.layoutResult = new LayoutResult() {
                        screenPosition = new Vector2(trackRect.x, trackRect.y),
                        actualSize = new Size(layoutResult.allocatedHeight, 5f),
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
                        m_Elements.Add(vertical);
                        onCreateVirtualScrollbar?.Invoke(vertical);
                        Extents childExtents = GetLocalExtents(box.children);
                        float offsetY = (childExtents.min.y < 0) ? -childExtents.min.y / box.allocatedHeight : 0f;
                        element.scrollOffset = new Vector2(element.scrollOffset.x, offsetY);
                    }

                    Rect trackRect = vertical.GetTrackRect();
                    vertical.layoutResult = new LayoutResult() {
                        screenPosition = new Vector2(trackRect.x, trackRect.y),
                        actualSize = new Size(layoutResult.allocatedHeight, 5f),
                        allocatedSize = new Size(box.actualHeight, 5f),
                    };
                }
            }

            if (m_RectUpdates.Count > 0) {
                Stack<UIElement> stack = StackPool<UIElement>.Get();

                stack.Push(root.children[0].element);

                while (stack.Count > 0) {
                    UIElement current = stack.Pop();

                    if (current.ownChildren == null) continue;

                    for (int i = 0; i < current.ownChildren.Length; i++) {
                        LayoutResult result = current.ownChildren[i].layoutResult;
                        result.screenPosition = current.layoutResult.screenPosition + result.localPosition;
                        current.ownChildren[i].layoutResult = result;
                        stack.Push(current.ownChildren[i]);
                    }
                }

                StackPool<UIElement>.Release(stack);
            }

            m_PendingWidthLayoutUpdates.Clear();
            m_PendingRectUpdates.Clear();
        }

        internal void OnRectChanged(LayoutBox layoutBox) {
            m_PendingRectUpdates.Add(layoutBox);
        }

        internal void PositionChanged(LayoutBox layoutBox) {
            m_PendingRectUpdates.Add(layoutBox);
        }

        internal void RequestWidthLayout(LayoutBox layoutBox) {
            if (layoutBox == root || layoutBox.markedForWidthLayout) return;
            m_PendingWidthLayoutUpdates.Add(layoutBox);
        }

        internal void RequestHeightLayout(LayoutBox layoutBox) {
            if (layoutBox == root || layoutBox.markedForHeightLayout) return;
            m_PendingHeightLayoutUpdates.Add(layoutBox);
        }

        public void OnDestroy() { }

        public void OnReady() {
            m_StyleSystem.onTextContentChanged += HandleTextContentChanged;
            m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
        }

        public void OnInitialize() { }

        private void HandleStylePropertyChanged(UIElement element, StyleProperty property) {
            
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) {
                return;
            }
            
            switch (property.propertyId) {
                case StylePropertyId.PreferredWidth:
                case StylePropertyId.PreferredHeight:
                    HandleSizeChanged(element);
                    break;
                case StylePropertyId.MinWidth:
                case StylePropertyId.MinHeight:
                case StylePropertyId.MaxWidth:
                case StylePropertyId.MaxHeight:
                    HandleSizeConstraintChanged(element);
                    break;
                case StylePropertyId.LayoutType:
                    HandleLayoutChanged(element);
                    break;
                case StylePropertyId.TransformPositionX:
                case StylePropertyId.TransformPositionY:
                    break;
            }

           
            box.OnStylePropertyChanged(property);
            box.parent?.OnChildStylePropertyChanged(box, property);
        }

        private void HandleSizeChanged(UIElement element) {
            if (element.parent == null) return;
            m_LayoutBoxMap.GetOrDefault(element.parent.id)?.OnChildSizeChanged();
        }

        private void HandleSizeConstraintChanged(UIElement element) {
            if (element.parent == null) return;
            m_LayoutBoxMap.GetOrDefault(element.id)?.OnSizeConstraintChanged(); // MarkForLayout(UpdateType.Constraint)
        }

        // todo -- eventually we can have this take a patch instead of a whole new string
        // that way we don't have to re-layout and re-measure parts of the string that didn't change
        private void HandleTextContentChanged(UIElement element, string content) {
            LayoutBox box;
            if (m_LayoutBoxMap.TryGetValue(element.id, out box)) {
               TextContainerLayoutBox textBox = box as TextContainerLayoutBox;
                if (textBox != null) {
                    textBox.OnTextContentUpdated();
                }
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
                return new TextContainerLayoutBox(this, element);
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnElementCreatedFromTemplate(MetaData elementData) {
            LayoutBox layoutBox = CreateLayoutBox(elementData.element);
            Stack<ValueTuple<MetaData, LayoutBox>> stack = StackPool<ValueTuple<MetaData, LayoutBox>>.Get();

            LayoutBox parent = root;
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

            m_PendingWidthLayoutUpdates.Add(layoutBox);
            layoutBox.markedForWidthLayout = true;
            m_Elements.Add(elementData.element);

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
                        stack.Push(ValueTuple.Create(childData, (LayoutBox)null));
                    }
                    else {
                        LayoutBox childBox = CreateLayoutBox(childData.element);
                        childBox.SetParent(parentBox);
                        m_LayoutBoxMap.Add(childData.element.id, childBox);
                        stack.Push(ValueTuple.Create(childData, childBox));
                        m_Elements.Add(childData.element);
                    }
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
            root.RunWidthLayout();
            root.RunHeightLayout();
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

        public List<UIElement> GetUpdatedLayoutElements(List<UIElement> retn) {
            retn = retn ?? ListPool<UIElement>.Get();
            for (int i = 0; i < m_RectUpdates.Count; i++) {
                retn.Add(m_RectUpdates[i].element);
            }

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