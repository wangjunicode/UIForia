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

        public readonly ITextSizeCalculator textCalculator; //= new GOTextSizeCalculator();

        public LayoutSystem2(ITextSizeCalculator textSizeCalculator, IStyleSystem styleSystem) {
            this.root = new RootLayoutBox(this);
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
                // if (m_UpdateRequiredElements[i].NeedsLayout) {
                m_UpdateRequiredElements[i].RunLayout();
                //     m_UpdateRequiredElements[i].dirtyFlags = 0;
                // }
            }

            m_RectUpdates.Sort((a, b) => a.element.depth > b.element.depth ? -1 : 1);

            for (int i = 0; i < m_RectUpdates.Count; i++) {
                LayoutBox box = m_RectUpdates[i];
                UIElement element = box.element;
                Vector2 localPosition = new Vector2(box.computedX, box.computedY);
                LayoutResult layoutResult = new LayoutResult(element);
                layoutResult.localPosition = localPosition;
                layoutResult.screenPosition = localPosition + box.parent.element.layoutResult.screenPosition;
                layoutResult.size = new Size(box.allocatedWidth, box.allocatedHeight);
                element.layoutResult = layoutResult;
            }

            m_UpdateRequiredElements.Clear();
        }

        internal void OnRectChanged(LayoutBox layoutBox) {
            m_RectUpdates.Add(layoutBox);
        }

        internal void RequestLayout(LayoutBox layoutBox) {
            // todo replace w/ set
            if (!m_UpdateRequiredElements.Contains(layoutBox)) {
                m_UpdateRequiredElements.Add(layoutBox);
            }
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
            m_StyleSystem.onLayoutDirectionChanged += HandleLayoutDirectionChanged;
            m_StyleSystem.onLayoutWrapChanged += HandleWrapStateChanged;
            //m_StyleSystem.onFlowStateChanged == HandleFlowStateChanged;
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
            m_LayoutBoxMap.GetOrDefault(element.id)?.OnContentRectChanged();
        }

        private void HandleLayoutDirectionChanged(UIElement element, LayoutDirection direction) {
            m_LayoutBoxMap.GetOrDefault(element.id)?.OnDirectionChanged(direction);    
        }
        
        private void HandleMainAxisAlignmentChanged(UIElement element, MainAxisAlignment newAlignment, MainAxisAlignment oldAlignment) {
            m_LayoutBoxMap.GetOrDefault(element.id)?.OnMainAxisAlignmentChanged(newAlignment, oldAlignment);
        }

        private void HandleCrossAxisAlignmentChanged(UIElement element, CrossAxisAlignment newAlignment, CrossAxisAlignment oldAlignment) {
            m_LayoutBoxMap.GetOrDefault(element.id)?.OnCrossAxisAlignmentChanged(newAlignment, oldAlignment);
        }

        private void HandleFlowStateChanged(UIElement element, LayoutFlowType flowType, LayoutFlowType oldFlow) {
            m_LayoutBoxMap.GetOrDefault(element.id)?.OnFlowStateChanged(flowType, oldFlow);
        }

        private void HandleWrapStateChanged(UIElement element, LayoutWrap newWrap, LayoutWrap oldWrap) {
            m_LayoutBoxMap.GetOrDefault(element.id)?.OnWrapChanged(newWrap, oldWrap);
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

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) { }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        private LayoutBox CreateLayoutBox(UIElement element) {
            
            if ((element is UITextContainerElement)) {
                return new TextContainerLayoutBox(textCalculator, this, element);
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
            
            while (stack.Count > 0) {
                ValueTuple<MetaData, LayoutBox> item = stack.Pop();
                MetaData parentData = item.Item1;
                LayoutBox parentBox = item.Item2;

                for (int i = 0; i < parentData.children.Count; i++) {
                    MetaData childData = parentData.children[i];
                    LayoutBox childBox = CreateLayoutBox(childData.element);
                    childBox.SetParent(parentBox);
                    m_LayoutBoxMap.Add(childData.element.id, layoutBox);
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
            root.allocatedWidth = viewportRect.width;
            root.minHeight = viewportRect.height;
            root.maxHeight = viewportRect.height;
            root.preferredHeight = viewportRect.height;
            root.allocatedHeight = viewportRect.height;
            //root.SetRectFromParentLayout(0, 0, viewportRect.width, viewportRect.height);
            root.RunLayout();
        }

        public List<UIElement> QueryPoint(Vector2 point, List<UIElement> retn) {
            return new List<UIElement>();
        }

        public List<LayoutResult> GetLayoutResults(List<LayoutResult> retn) {
            return new List<LayoutResult>();
        }

    }

}