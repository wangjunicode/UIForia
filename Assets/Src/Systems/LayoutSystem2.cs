using System;
using System.Collections.Generic;
using Rendering;
using Src.Elements;
using Src.Layout;
using Src.Util;
using UnityEngine;

namespace Src.Systems {

    public class LayoutSystem2 : ILayoutSystem {

        public event Action<VirtualScrollbar> onCreateVirtualScrollbar;
        public event Action<VirtualScrollbar> onDestroyVirtualScrollbar;

        protected LayoutBox root;
        protected readonly List<LayoutBox> m_UpdateRequiredElements;
        protected readonly List<LayoutBox> m_RectUpdates;
        protected readonly Dictionary<int, LayoutBox> m_LayoutBoxMap;
        protected readonly IStyleSystem m_StyleSystem;

        public readonly ITextSizeCalculator textCalculator; //= new GOTextSizeCalculator();

        public LayoutSystem2(ITextSizeCalculator textSizeCalculator, IStyleSystem styleSystem) {
            this.root = new LayoutBox(this, null);
            this.textCalculator = textSizeCalculator;
            this.m_StyleSystem = styleSystem;
            this.m_LayoutBoxMap = new Dictionary<int, LayoutBox>();
            this.m_UpdateRequiredElements = new List<LayoutBox>();
            this.m_RectUpdates = new List<LayoutBox>();
        }

        public Rect ViewportRect { get; private set; }

        public void OnReset() { }

        public void OnUpdate() {

            m_RectUpdates.Clear();

            if (m_UpdateRequiredElements.Count == 0) return;
           
            m_UpdateRequiredElements.Sort((a, b) => a.element.depth > b.element.depth ? -1 : 1);
            
            for (int i = 0; i < m_UpdateRequiredElements.Count; i++) {
                
                if (m_UpdateRequiredElements[i].NeedsLayout) {
                    m_UpdateRequiredElements[i].RunLayout();
                    m_UpdateRequiredElements[i].dirtyFlags = 0;
                }
              
            }
         
            m_RectUpdates.Sort((a, b) => a.element.depth > b.element.depth ? -1 : 1);
            
            for (int i = 0; i < m_RectUpdates.Count; i++) {
                LayoutBox box = m_RectUpdates[i];
                UIElement element = box.element;
            }
            
            m_UpdateRequiredElements.Clear();
        }

        internal void OnRectChanged(LayoutBox layoutBox) {
            m_RectUpdates.Add(layoutBox);
        }
        
        internal void RequestLayout(LayoutBox layoutBox) {
            if (!m_UpdateRequiredElements.Contains(layoutBox)) {
                m_UpdateRequiredElements.Add(layoutBox);
            }
        }

        public Layout2 GetLayout(LayoutType layoutType) {
            switch (layoutType) {
                case LayoutType.Flex:
                    return new FlexLayout2();
            }

            return null;
        }

        public void OnDestroy() { }

        public void OnReady() { }

        public void OnInitialize() {
//            m_StyleSystem.onOverflowPropertyChanged += HandleOverflowChanged;
            m_StyleSystem.onBorderChanged += HandleContentBoxChanged;
            m_StyleSystem.onPaddingChanged += HandleContentBoxChanged;
            m_StyleSystem.onMarginChanged += HandleContentBoxChanged;
            m_StyleSystem.onLayoutChanged += HandleLayoutChanged;
            m_StyleSystem.onRectChanged += HandleRectChanged;
            m_StyleSystem.onTextContentChanged += HandleTextContentChanged;
        }

        private void HandleFontPropertyChanged(UIElement element, TextStyle textStyle) {
            LayoutBox box;
            if (m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                bool neededLayout = box.NeedsLayout;
                if (box.isTextElement) {
                    // box.SetTextSize(textCalculator.CalcTextPreferredAndMinWidth(element.style));
                }
                else {
                    box.SetFontSize(textStyle.fontSize);
                }

                if (!neededLayout && box.NeedsLayout) {
                    m_UpdateRequiredElements.Add(box);
                }
            }
        }

        private void HandleTextContentChanged(UIElement element, string content) {
            LayoutBox box;
            if (m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                bool neededLayout = box.NeedsLayout;
                // box.SetTextSize(textCalculator.CalcTextPreferredAndMinWidth(element.style));
                if (!neededLayout && box.NeedsLayout) {
                    m_UpdateRequiredElements.Add(box);
                }
            }
        }

        private void HandleRectChanged(UIElement element, Dimensions d) {
            LayoutBox box;
            if (m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                bool neededLayout = box.NeedsLayout;
                box.SetSize(d);
                if (!neededLayout && box.NeedsLayout) {
                    m_UpdateRequiredElements.Add(box);
                }
            }
        }

        private void HandleLayoutChanged(UIElement element, LayoutParameters layoutParameters) {
            LayoutBox box;
            if (m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                bool neededLayout = box.NeedsLayout;
                box.SetLayoutParameters(layoutParameters);
                if (!neededLayout && box.NeedsLayout) {
                    m_UpdateRequiredElements.Add(box);
                }
            }
        }

        private void HandleContentBoxChanged(UIElement element, ContentBoxRect contentBox) {
            LayoutBox box;
            if (m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                bool neededLayout = box.NeedsLayout;
                box.SetContentBox(contentBox);
                if (!neededLayout && box.NeedsLayout) {
                    m_UpdateRequiredElements.Add(box);
                }
            }
        }

        public void OnElementCreated(UIElement element) { }

        public void OnElementMoved(UIElement element, int newIndex, int oldIndex) { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        public void OnElementCreatedFromTemplate(MetaData elementData) {
            // todo pool
            LayoutBox layoutBox = new LayoutBox(this, elementData.element);
            Stack<ValueTuple<MetaData, LayoutBox>> stack = StackPool<ValueTuple<MetaData, LayoutBox>>.Get();

            LayoutBox parent = root;
            if (elementData.element.parent != null) {
                if (!m_LayoutBoxMap.TryGetValue(elementData.element.parent.id, out parent)) {
                    parent = root;
                }
            }

            layoutBox.SetParent(parent);

            stack.Push(ValueTuple.Create(elementData, layoutBox));

            while (stack.Count > 0) {
                ValueTuple<MetaData, LayoutBox> item = stack.Pop();
                MetaData parentData = item.Item1;
                LayoutBox parentBox = item.Item2;

                for (int i = 0; i < parentData.children.Count; i++) {
                    MetaData childData = parentData.children[i];
                    LayoutBox childBox = new LayoutBox(this, childData.element);
                    childBox.SetParent(parentBox);
                    m_LayoutBoxMap.Add(childData.element.id, childBox);
                    stack.Push(ValueTuple.Create(childData, childBox));
                }
            }

            StackPool<ValueTuple<MetaData, LayoutBox>>.Release(stack);
        }

        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) { }


        public void SetViewportRect(Rect viewportRect) {
            ViewportRect = viewportRect;
            root.minWidth = viewportRect.width;
            root.maxWidth = viewportRect.width;
            root.preferredWidth = viewportRect.width;
            root.computedWidth = viewportRect.width;
            root.minHeight = viewportRect.height;
            root.maxHeight = viewportRect.height;
            root.preferredHeight = viewportRect.height;
            root.computedHeight = viewportRect.height;
            root.dirtyFlags = 0;
        }

        public List<UIElement> QueryPoint(Vector2 point, List<UIElement> retn) {
            return new List<UIElement>();
        }

        public List<LayoutResult> GetLayoutResults(List<LayoutResult> retn) {
            return new List<LayoutResult>();
        }

    }

}