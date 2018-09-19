using System;
using System.Collections;
using System.Collections.Generic;
using Rendering;
using Src.Elements;
using Src.Layout;
using Src.Util;
using UnityEngine;
using TreeChangeType = Src.SkipTree<UIElement>.TreeChangeType;

namespace Src.Systems {

    public class LayoutSystem : ILayoutSystem {

        private readonly FlexLayout flexLayout;
        private readonly FlowLayout flowLayout;
        private readonly FixedLayout fixedLayout;
        private readonly IStyleSystem styleSystem;
        private readonly SkipTree<LayoutNode> layoutTree;
        private readonly List<VirtualScrollbar> m_VirtualScrollBars;

        private int rectCount;
        private Rect viewport;
        private bool isReady;
        private LayoutResult[] rects;

        public event Action<VirtualScrollbar> onCreateVirtualScrollbar;
        public event Action<VirtualScrollbar> onDestroyVirtualScrollbar;
        
        private SkipTree<LayoutNode>.TreeNode tree;
        private bool m_TreeUpdateRequired;
        
        public LayoutSystem(ITextSizeCalculator textSizeCalculator, IStyleSystem styleSystem) {
            this.styleSystem = styleSystem;
            this.layoutTree = new SkipTree<LayoutNode>();

            this.flexLayout = new FlexLayout(textSizeCalculator);
            this.fixedLayout = new FixedLayout(textSizeCalculator);

            this.rects = new LayoutResult[16];
            this.m_VirtualScrollBars = new List<VirtualScrollbar>();
            this.layoutTree.onItemParentChanged += HandleTreeParentChanged;
            this.m_TreeUpdateRequired = true;
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

        public void OnElementCreated(UIElement element) { }

        public void OnElementMoved(UIElement element, int newIndex, int oldIndex) { }

        public void OnReady() {
            isReady = true;
            layoutTree.TraversePreOrder(this, (self, node) => { node.UpdateData(self); });
        }

        public void OnElementCreatedFromTemplate(MetaData elementData) {
            m_TreeUpdateRequired = true;
            if ((elementData.element.flags & UIElementFlags.RequiresLayout) != 0) {
                LayoutNode node = new LayoutNode(elementData.element);
                layoutTree.AddItem(node);

                if (isReady) {
                    node.UpdateData(this);
                }
            }

            for (int i = 0; i < elementData.children.Count; i++) {
                OnElementCreatedFromTemplate(elementData.children[i]);
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

//            LayoutNode node0 = layoutTree.GetRootItems()[0];
//            node0.outputRect = viewport;

            if (m_TreeUpdateRequired) {
                tree = layoutTree.GetTraversableTree().children[0];
                m_TreeUpdateRequired = false;
            }

            tree.item.layout.Run(viewport, tree.item);

            Stack<SkipTree<LayoutNode>.TreeNode> nodeStack = StackPool<SkipTree<LayoutNode>.TreeNode>.Get();
            nodeStack.Push(tree);

            tree.item.element.localPosition = Vector2.zero;
            tree.item.element.screenPosition = new Vector2(viewport.x, viewport.y);
            tree.item.element.width = viewport.width;
            tree.item.element.height = viewport.height;

            while (nodeStack.Count > 0) {
                SkipTree<LayoutNode>.TreeNode current = nodeStack.Pop();
                UIElement element = current.item.element;
                if (element.isDisabled || (element.flags & UIElementFlags.Destroyed) != 0) {
                    continue;
                }

                rects[rectCount++] = new LayoutResult(
                    element,
                    new Rect(
                        element.screenPosition.x,
                        element.screenPosition.y,
                        element.width,
                        element.height
                    )
                );

                current.item.layout.Run(viewport, current.item);

                for (int i = 0; i < current.children.Length; i++) {
                    nodeStack.Push(current.children[i]);
                }

                if (!element.style.HandlesOverflow) {
                    continue;
                }

                Extents childExtents = current.item.GetChildExtents();

                if (element.style.HandlesOverflowX) {

                    bool isOverflowing = childExtents.min.x < element.screenPosition.x || childExtents.max.x > element.screenPosition.x + element.width;

                    if (isOverflowing) {
                        VirtualScrollbar horizontal;
                        if (current.item.horizontalScrollbar == null) {
                            horizontal = new VirtualScrollbar(element, ScrollbarOrientation.Horizontal);
                            horizontal.depth = element.depth;
                            horizontal.siblingIndex = int.MaxValue;
                            m_VirtualScrollBars.Add(horizontal);
                            onCreateVirtualScrollbar?.Invoke(horizontal);
                            current.item.horizontalScrollbar = horizontal;
                        }
                        else {
                            horizontal = current.item.horizontalScrollbar;
                        }

                        horizontal.SetHandleSize(20f);
                    }
                    else if (current.item.horizontalScrollbar != null) {
                        onDestroyVirtualScrollbar?.Invoke(current.item.horizontalScrollbar);
                        current.item.horizontalScrollbar = null;
                    }
                }

                if (element.style.HandlesOverflowY) {

                    bool isOverflowing = childExtents.min.y < element.screenPosition.y || childExtents.max.y > element.screenPosition.y + element.height;

                    if (isOverflowing) {
                        VirtualScrollbar vertical;
                        if (current.item.verticalScrollbar == null) {
                            vertical = new VirtualScrollbar(element, ScrollbarOrientation.Vertical);
                            vertical.depth = element.depth;
                            vertical.siblingIndex = int.MaxValue;
                            onCreateVirtualScrollbar?.Invoke(vertical);
                            m_VirtualScrollBars.Add(vertical);
                            current.item.verticalScrollbar = vertical;
                        }
                        else {
                            vertical = current.item.verticalScrollbar;
                        }

                        Rect trackRect = vertical.GetTrackRect();
                        float contentHeight = childExtents.max.y - element.screenPosition.y;
                        vertical.contentHeight = contentHeight;
                        vertical.screenPosition = new Vector2(trackRect.x, trackRect.y);
                        vertical.width = 5f;
                        vertical.height = element.height;
                        
                        vertical.SetHandleSize(20f);
                    }
                    else if (current.item.verticalScrollbar != null) {
                        onDestroyVirtualScrollbar?.Invoke(current.item.verticalScrollbar);
                        current.item.verticalScrollbar = null;
                    }
                }
            }

            StackPool<SkipTree<LayoutNode>.TreeNode>.Release(nodeStack);
        }       

        public bool GetRectForElement(int elementId, out Rect rect) {
            LayoutNode node = layoutTree.GetItem(elementId);
            if (node != null) {
                UIElement element = node.element;
                rect = new Rect(
                    element.screenPosition.x,
                    element.screenPosition.y,
                    element.width,
                    element.height
                );
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

            for (int i = 0; i < m_VirtualScrollBars.Count; i++) {
                if (m_VirtualScrollBars[i].ScreenRect.Contains(point)) {
                    queryResults[retnCount++] = new LayoutResult(m_VirtualScrollBars[i], m_VirtualScrollBars[i].ScreenRect);
                }
            }

            return retnCount;
        }

        public void OnReset() {
            m_TreeUpdateRequired = true;
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

        public void OnElementEnabled(UIElement element) {
            m_TreeUpdateRequired = true;
        }

        public void OnElementDisabled(UIElement element) {
            m_TreeUpdateRequired = true;
        }

        public void OnElementDestroyed(UIElement element) {
            m_TreeUpdateRequired = true;
            layoutTree.RemoveHierarchy(element);
        }

        public void OnElementShown(UIElement element) { }

        public void OnElementHidden(UIElement element) { }

        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) {
            layoutTree.UpdateItemParent(element);
            m_TreeUpdateRequired = true;
        }

        private void HandleFontPropertyChanged(UIElement element, TextStyle textStyle) {
            LayoutNode node = layoutTree.GetItem(element);
            node?.UpdateTextMeasurements();
        }

        private void HandleLayoutChanged(UIElement element, LayoutParameters parameters) {
            LayoutNode node = layoutTree.GetItem(element);
            if (node == null) return;
            node.layout = GetLayoutInstance(parameters.type);
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