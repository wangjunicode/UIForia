using System;
using System.Collections.Generic;
using Rendering;
using Src.Layout;
using UnityEngine;
using TreeChangeType = Src.SkipTree<UIElement>.TreeChangeType;

namespace Src.Systems {

    public class LayoutSystem : ILayoutSystem {

        private readonly IStyleSystem styleSystem;
        private readonly SkipTree<UIElement> layoutTree;
        private readonly Stack<LayoutNode> layoutStack;
        private readonly Dictionary<int, LayoutNode> layoutDataMap;

        // todo -- use simple array for this w/ unique ids -> indices on elements
        private readonly Dictionary<int, Rect> layoutResultMap;

        private readonly FlexLayout flexLayout;
        private readonly FlowLayout flowLayout;
        private readonly FixedLayout fixedLayout;

        private int rectCount;

        // todo -- remove layoutrects
        private Rect viewport;
        private Rect[] layoutRects;
        private LayoutResult[] rects;
        private LayoutNode traverseTree;

        private bool rebuildLayout;

        /*
         * It is possible to have a scheme where element ids can also contain an integer index into lists
         * To do this we need to maintain a list of available ids a-la bitsquid packed list
         */
        public LayoutSystem(ITextSizeCalculator textSizeCalculator, IStyleSystem styleSystem) {
            this.styleSystem = styleSystem;
            this.layoutRects = new Rect[16];
            this.layoutStack = new Stack<LayoutNode>();
            this.layoutTree = new SkipTree<UIElement>();
            this.layoutResultMap = new Dictionary<int, Rect>();
            this.layoutDataMap = new Dictionary<int, LayoutNode>();

            this.styleSystem.onRectChanged += HandleRectChanged;
            this.styleSystem.onLayoutChanged += HandleLayoutChanged;
            this.styleSystem.onBorderChanged += HandleContentBoxChanged;
            this.styleSystem.onMarginChanged += HandleContentBoxChanged;
            this.styleSystem.onPaddingChanged += HandleContentBoxChanged;
            this.styleSystem.onConstraintChanged += HandleConstraintChanged;

            this.flexLayout = new FlexLayout(textSizeCalculator);
            this.rects = new LayoutResult[16];

            this.layoutTree.onTreeChanged += RebuildTree;
            rebuildLayout = true;
        }

        // todo -- rebuild once right before layout is actually called
        private void RebuildTree(TreeChangeType changeType) {
            switch (changeType) {
                case TreeChangeType.ItemAdded:
                    break;
                case TreeChangeType.HierarchyDisabled:
                    break;
                case TreeChangeType.HierarchyRemoved:
                    break;
                case TreeChangeType.HierarchyEnabled:
                    break;
                case TreeChangeType.Cleared:
                    break;
            }
            rebuildLayout = true;
            // todo make this suck less later
            // todo just use fucking style object w/ cache instead of jumping through so many hoops

        }

        private LayoutNode[] GetChildTree(SkipTree<UIElement>.TreeNode parent) {
            LayoutNode[] children = new LayoutNode[parent.children.Length];
            for (int i = 0; i < parent.children.Length; i++) {
                UIElement element = parent.children[i].item;
                LayoutNode child = new LayoutNode(element, element.id, GetChildTree(parent.children[i]));
                layoutDataMap[child.elementId] = child;
                SetFromStyle(child, element);
                children[i] = child;
            }
            return children;
        }

        private void SetFromStyle(LayoutNode node, UIElement element) {
            UIStyleSet style = styleSystem.GetStyleForElement(node.elementId);

            layoutDataMap[node.elementId] = node;

            if ((element.flags & UIElementFlags.TextElement) != 0) {
                UITextElement textElement = (UITextElement) element;
                textElement.onTextChanged += HandleTextChanged;
                node.textContent = textElement.GetText();
                node.isTextElement = true;
            }

            node.style = element.style;

            node.previousParentWidth = float.MinValue;
            node.textContentSize = Vector2.zero;

            node.contentStartOffsetX = style.paddingLeft + style.marginLeft + style.borderLeft;
            node.contentEndOffsetX = style.paddingRight + style.marginRight + style.borderRight;

            node.contentStartOffsetY = style.paddingTop + style.marginTop + style.borderTop;
            node.contentEndOffsetY = style.paddingBottom + style.marginBottom + style.borderBottom;

            node.parameters = style.layoutParameters;
            node.layout = GetLayoutInstance(node.parameters.type);

            node.constraints = style.constraints;
            node.rect = style.rect;

        }

        private void HandleRectChanged(int elementId, LayoutRect rect) {
            LayoutNode data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.rect = rect;
            }
        }

        private void HandleConstraintChanged(int elementId, LayoutConstraints constraints) {
            LayoutNode data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.constraints = constraints;
            }
        }

        // todo -- change to contentbox changed 
        private void HandleContentBoxChanged(int elementId, ContentBoxRect padding) {
            LayoutNode data;
            UIStyleSet style = styleSystem.GetStyleForElement(elementId);
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.contentStartOffsetX = style.paddingLeft + style.marginLeft + style.borderLeft;
                data.contentEndOffsetX = style.paddingRight + style.marginRight + style.borderRight;
                data.contentStartOffsetY = style.paddingTop + style.marginTop + style.borderTop;
                data.contentEndOffsetY = style.paddingBottom + style.marginBottom + style.borderBottom;
            }
        }

        private void HandleLayoutChanged(int elementId, LayoutParameters parameters) {
            LayoutNode data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.parameters = parameters;
                data.layout = GetLayoutInstance(parameters.type);
            }
        }

        private void HandleTextChanged(UIElement element, string text) {
            LayoutNode layoutData;
            if (layoutDataMap.TryGetValue(element.id, out layoutData)) {
                layoutData.SetTextContent(text);
            }
        }

        public void OnInitialize() {
            rebuildLayout = true;
        }

        public void OnElementCreated(UIElementCreationData elementData) {

            if (elementData.element.style == null) return;

            if ((elementData.element.flags & UIElementFlags.RequiresLayout) == 0) {
                return;
            }

            rebuildLayout = true;
            if ((elementData.element.flags & UIElementFlags.TextElement) != 0) {
                UITextElement textElement = (UITextElement) elementData.element;
                textElement.onTextChanged += HandleTextChanged;
            }

            layoutTree.AddItem(elementData.element);
        }

        private UILayout GetLayoutInstance(LayoutType layoutType) {
            switch (layoutType) {
                case LayoutType.Flex:
                    return flexLayout;
            }
            throw new NotImplementedException();
        }

        public int RectCount => rectCount;
        public LayoutResult[] LayoutResults => rects;

        public void SetViewportRect(Rect viewport) {
            this.viewport = viewport;
        }

        public void OnReset() {
            layoutDataMap.Clear();
            layoutTree.Clear();
            rebuildLayout = true;
        }

        public Rect GetRectForElement(int elementId) {
            Rect retn;
            layoutResultMap.TryGetValue(elementId, out retn);
            return retn;
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

        public void OnUpdate() {

            if (rebuildLayout) {
                SkipTree<UIElement>.TreeNode tree = layoutTree.GetTraversableTree();
                if (tree.children.Length != 0) {
                    traverseTree = new LayoutNode(tree.children[0].item, tree.children[0].item.UniqueId, GetChildTree(tree.children[0]));
                    SetFromStyle(traverseTree, tree.children[0].item);
                    layoutDataMap[traverseTree.elementId] = traverseTree;
                }
                rebuildLayout = false;
            }

            if (!rebuildLayout && traverseTree == null) {
                return;
            }

            rectCount = 0;
            layoutResultMap.Clear();

            if (rects.Length <= layoutTree.Size) {
                Array.Resize(ref rects, layoutTree.Size * 2);
                Array.Resize(ref layoutRects, layoutTree.Size * 2);
            }

            traverseTree.outputRect = viewport;

            layoutStack.Push(traverseTree);

            while (layoutStack.Count > 0) {
                LayoutNode currentTreeNode = layoutStack.Pop();

                // layout rects gets filled from zero every layout iteration
                // rects is actual final position after layout is completed
                currentTreeNode.layout.Run(viewport, currentTreeNode, layoutRects);

                layoutResultMap[currentTreeNode.elementId] = currentTreeNode.outputRect;

                rects[rectCount++] = new LayoutResult(currentTreeNode.elementId, currentTreeNode.outputRect);

                // note: we never need to clear the layoutResults array
                for (int i = 0; i < currentTreeNode.children.Length; i++) {
                    currentTreeNode.children[i].outputRect = layoutRects[i];
                    layoutStack.Push(currentTreeNode.children[i]);
                }
            }
        }

        public void OnDestroy() {
            this.styleSystem.onRectChanged -= HandleRectChanged;
            this.styleSystem.onLayoutChanged -= HandleLayoutChanged;
            this.styleSystem.onBorderChanged -= HandleContentBoxChanged;
            this.styleSystem.onMarginChanged -= HandleContentBoxChanged;
            this.styleSystem.onPaddingChanged -= HandleContentBoxChanged;
            this.styleSystem.onConstraintChanged -= HandleConstraintChanged;
            layoutDataMap.Clear();
            layoutTree.Clear();
        }

        public void OnElementEnabled(UIElement element) {
            layoutTree.EnableHierarchy(element);
        }

        public void OnElementDisabled(UIElement element) {
            layoutTree.EnableHierarchy(element);
        }

        public void OnElementDestroyed(UIElement element) {
            // todo -- recurse this hierarchy and do a proper destroy call on each
            layoutTree.RemoveHierarchy(element);
            if ((element.flags & UIElementFlags.TextElement) != 0) {
                UITextElement textElement = (UITextElement) element;
                textElement.onTextChanged -= HandleTextChanged;
            }
        }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

    }

}