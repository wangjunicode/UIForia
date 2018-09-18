using System;
using Rendering;
using Src.Layout;
using UnityEngine;
using TreeChangeType = Src.SkipTree<UIElement>.TreeChangeType;

namespace Src.Systems {

    public class LayoutSystem : ILayoutSystem {

        private readonly FlexLayout flexLayout;
        private readonly FlowLayout flowLayout;
        private readonly FixedLayout fixedLayout;
        private readonly IStyleSystem styleSystem;
        private readonly SkipTree<LayoutNode> layoutTree;

        private int rectCount;
        private Rect viewport;
        private bool isReady;
        private LayoutResult[] rects;

        public LayoutSystem(ITextSizeCalculator textSizeCalculator, IStyleSystem styleSystem) {
            this.styleSystem = styleSystem;
            this.layoutTree = new SkipTree<LayoutNode>();

            this.flexLayout = new FlexLayout(textSizeCalculator);
            this.fixedLayout = new FixedLayout(textSizeCalculator);
            
            this.rects = new LayoutResult[16];

            this.layoutTree.onItemParentChanged += HandleTreeParentChanged;
        }

        public int RectCount => rectCount;
        public LayoutResult[] LayoutResults => rects;

        public void SetViewportRect(Rect viewport) {
            this.viewport = viewport;
        }

        public void OnInitialize() {
            this.styleSystem.onLayoutChanged += HandleLayoutChanged;
            this.styleSystem.onTextContentChanged += HandleTextChanged;
            this.styleSystem.onRectChanged += HandleRectChanged;
            this.styleSystem.onBorderChanged += HandleContentBoxChanged;
            this.styleSystem.onPaddingChanged += HandleContentBoxChanged;
            this.styleSystem.onMarginChanged += HandleContentBoxChanged;
            this.styleSystem.onConstraintChanged += HandleConstraintChanged;
            this.styleSystem.onFontPropertyChanged += HandleFontPropertyChanged;
        }

        public void OnReady() {
            isReady = true;
            layoutTree.TraversePreOrder(this, (self, node) => { node.UpdateData(self); });
        }

        public void OnElementCreated(MetaData elementData) {
            if ((elementData.element.flags & UIElementFlags.RequiresLayout) != 0) {
                LayoutNode node = new LayoutNode(elementData.element);
                layoutTree.AddItem(node);

                if (isReady) {
                    node.UpdateData(this);
                }
            }

            for (int i = 0; i < elementData.children.Count; i++) {
                OnElementCreated(elementData.children[i]);
            }
        }

        public UILayout GetLayoutInstance(LayoutType layoutType) {
            switch (layoutType) {
                case LayoutType.Flex:
                    return flexLayout;
                case LayoutType.Fixed:
                    return fixedLayout;
            }

            throw new NotImplementedException();
        }


        public void OnUpdate() {
            rectCount = 0;
            if (rects.Length <= layoutTree.Size) {
                Array.Resize(ref rects, layoutTree.Size * 2);
            }

            LayoutNode node0 = layoutTree.GetRootItems()[0];
            node0.outputRect = viewport;
            layoutTree.ConditionalTraversePreOrder(this, (node, self) => {
                if ((node.element.flags & UIElementFlags.Destroyed) != 0) {
                    return false;
                }

                if (node.element.isDisabled) return false;
                
                self.rects[self.rectCount++] = new LayoutResult(
                    node.element,
                    node.outputRect,
                    new Rect(node.localPosition, node.outputRect.size)
                );

                ElementMeasurements measurements = node.element.measurements;
                measurements.childExtents = node.GetChildExtents();
                measurements.viewPosition = Vector2.zero;
                measurements.screenPosition = Vector2.zero;
                measurements.localPosition = node.localPosition;
                        
                measurements.width = node.outputRect.width;
                measurements.height = node.outputRect.height;
                
                if (node.isTextElement) {
                    return true;
                }

                node.layout.Run(self.viewport, node);
                return true;
            });
        }

        public bool GetRectForElement(int elementId, out Rect rect) {
            LayoutNode node = layoutTree.GetItem(elementId);
            if (node != null) {
                rect = node.outputRect;
                return true;
            }

            rect = default(Rect);
            return false;
        }

        // todo this needs to be replaced with a quad tree eventually
        public int QueryPoint(Vector2 point, ref LayoutResult[] queryResults) {
            int retnCount = 0;
            for (int i = 0; i < rectCount; i++) {

                if (!rects[i].rect.Contains(point)) {
                    continue;
                }
                
                if (rectCount == queryResults.Length) {
                    Array.Resize(ref queryResults, queryResults.Length * 2);
                }

                queryResults[retnCount++] = rects[i];
            }

            return retnCount;
        }

        public void OnReset() {
            OnDestroy();
        }

        public void OnDestroy() {
            this.styleSystem.onFontPropertyChanged -= HandleFontPropertyChanged;
            this.styleSystem.onLayoutChanged -= HandleLayoutChanged;
            this.styleSystem.onTextContentChanged -= HandleTextChanged;
            this.styleSystem.onRectChanged -= HandleRectChanged;
            this.styleSystem.onBorderChanged -= HandleContentBoxChanged;
            this.styleSystem.onPaddingChanged -= HandleContentBoxChanged;
            this.styleSystem.onMarginChanged -= HandleContentBoxChanged;
            this.styleSystem.onConstraintChanged -= HandleConstraintChanged;
            layoutTree.Clear();
        }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) { }

        public void OnElementDestroyed(UIElement element) {
            layoutTree.RemoveHierarchy(element);
        }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }
        
        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) {
            layoutTree.UpdateItemParent(element);
        }

        private void HandleFontPropertyChanged(UIElement element, TextStyle textStyle) {
            LayoutNode node = layoutTree.GetItem(element);
            node?.UpdateTextMeasurements();
        }

        private void HandleLayoutChanged(UIElement element, LayoutParameters parameters) {
            LayoutNode node = layoutTree.GetItem(element);
            if (node == null) return;
            node.layout = GetLayoutInstance(parameters.type);
            node.parameters = parameters;
        }

        private void HandleTextChanged(UIElement element, string text) {
            LayoutNode node = layoutTree.GetItem(element);
            node?.SetTextContent(text);
        }

        private static void HandleTreeParentChanged(LayoutNode child, LayoutNode newParent, LayoutNode oldParent) {
            oldParent?.children.Remove(child);
            newParent?.children.Add(child);
        }

        private void HandleConstraintChanged(UIElement element, LayoutConstraints constraints) {
            LayoutNode node = layoutTree.GetItem(element);
            node?.UpdateData(this);
        }

        private void HandleContentBoxChanged(UIElement element, ContentBoxRect rect) {
            LayoutNode node = layoutTree.GetItem(element);
            node?.UpdateData(this);
        }

        private void HandleRectChanged(UIElement element, Dimensions rect) {
            LayoutNode node = layoutTree.GetItem(element);
            node?.UpdateData(this);
        }

    }

}