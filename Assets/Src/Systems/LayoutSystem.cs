using System;
using Rendering;
using Src.Layout;
using UnityEngine;
using TreeChangeType = Src.SkipTree<UIElement>.TreeChangeType;

namespace Src.Systems {

    public class LayoutSystem : ILayoutSystem {

        private readonly IStyleSystem styleSystem;
        private readonly SkipTree<LayoutNode> layoutTree;

        // todo -- use simple array for this w/ unique ids -> indices on elements

        private readonly FlexLayout flexLayout;
        private readonly FlowLayout flowLayout;
        private readonly FixedLayout fixedLayout;

        private int rectCount;
        private Rect viewport;
        private LayoutResult[] rects;
        private bool isReady;
        
        public LayoutSystem(ITextSizeCalculator textSizeCalculator, IStyleSystem styleSystem) {
            this.styleSystem = styleSystem;
            this.layoutTree = new SkipTree<LayoutNode>();
            this.styleSystem.onLayoutChanged += HandleLayoutChanged;

            this.styleSystem.onTextContentChanged += HandleTextChanged;

            this.flexLayout = new FlexLayout(textSizeCalculator);
            this.rects = new LayoutResult[16];

            this.layoutTree.onItemParentChanged += HandleTreeParentChanged;
        }


        public int RectCount => rectCount;
        public LayoutResult[] LayoutResults => rects;

       
        public void OnInitialize() { }

        public void OnReady() {
            isReady = true;
            layoutTree.TraversePreOrder(this, (self, node) => { node.UpdateData(self); });
        }

        public void OnElementCreated(InitData elementData) {
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
            }

            throw new NotImplementedException();
        }


        public void SetViewportRect(Rect viewport) {
            this.viewport = viewport;
        }

        public void OnUpdate() {
            rectCount = 0;

            if (rects.Length <= layoutTree.Size) {
                Array.Resize(ref rects, layoutTree.Size * 2);
            }

            LayoutNode node0 = layoutTree.GetRootItems()[0];
            node0.outputRect = viewport;
            layoutTree.ConditionalTraversePreOrder((node) => {
                if ((node.element.flags & UIElementFlags.Destroyed) != 0) {
                    return false;
                }
                if (node.element.isDisabled) return false;
                rects[rectCount++] = new LayoutResult(node.element.id, node.outputRect, new Rect(node.localPosition, node.outputRect.size));
                if (node.isTextElement) {
                    return true;
                }
                node.layout.Run(viewport, node, null);
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
                if (rects[i].rect.Contains(point)) {
                    if (rectCount == queryResults.Length) {
                        Array.Resize(ref queryResults, queryResults.Length * 2);
                    }

                    queryResults[retnCount++] = rects[i];
                }
            }

            return retnCount;
        }

        public void OnReset() {
            OnDestroy();
        }

        public void OnDestroy() {
            this.styleSystem.onLayoutChanged -= HandleLayoutChanged;
            this.styleSystem.onTextContentChanged -= HandleTextChanged;
            layoutTree.Clear();
        }

        public void OnElementEnabled(UIElement element) {}

        public void OnElementDisabled(UIElement element) {}

        public void OnElementDestroyed(UIElement element) {
            layoutTree.RemoveHierarchy(element);
        }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        private void HandleLayoutChanged(int elementId, LayoutParameters parameters) {
            LayoutNode node = layoutTree.GetItem(elementId);
            if (node == null) return;
            node.layout = GetLayoutInstance(parameters.type);
        }

        private void HandleTextChanged(int elementId, string text) {
            LayoutNode node = layoutTree.GetItem(elementId);
            node?.SetTextContent(text);
        }
        
        private static void HandleTreeParentChanged(LayoutNode child, LayoutNode newParent, LayoutNode oldParent) {
            oldParent?.children.Remove(child);
            newParent?.children.Add(child);
        }

    }

}
