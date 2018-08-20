using System;
using System.Collections.Generic;
using Rendering;
using Src.Layout;
using UnityEngine;

namespace Src.Systems {

    public class LayoutSystem : ISystem {

        private Stack<LayoutDataSet> layoutStack;
        private SkipTree<LayoutData> layoutTree;
        private Rect[] layoutRects;

        public LayoutSystem() {
            this.layoutStack = new Stack<LayoutDataSet>();
            this.layoutTree = new SkipTree<LayoutData>();
            this.layoutRects = new Rect[16];
        }

        public void OnElementCreated(UIElementCreationData elementData) {
            // todo -- if instance is layout-able
            // todo -- temporary
            LayoutData data = new LayoutData(elementData.element);
            data.layoutType = LayoutType.Flex;
            data.crossAxisAlignment = CrossAxisAlignment.Stretch;
            data.layoutDirection = LayoutDirection.Row;
            layoutTree.AddItem(data);
        }

        public int RunLayout(Rect viewport, ref LayoutResult[] output) {

            layoutTree.TraversePreOrderWithCallback(output, SetupLayoutPass);

            LayoutData[] roots = layoutTree.GetRootItems();
            // todo -- there needs to be pseudo root in the layout tree in order to handle layout of root level things

            LayoutData pseudoRoot = new LayoutData(null);
            pseudoRoot.layoutDirection = LayoutDirection.Column;
            pseudoRoot.mainAxisAlignment = MainAxisAlignment.Default;
            pseudoRoot.crossAxisAlignment = CrossAxisAlignment.Stretch;
            pseudoRoot.layoutType = LayoutType.Flex;

            pseudoRoot.preferredWidth = viewport.width;
            pseudoRoot.preferredWidth = viewport.height;
            pseudoRoot.maxWidth = float.MaxValue;
            pseudoRoot.maxHeight = float.MaxValue;
            pseudoRoot.minWidth = viewport.width;
            pseudoRoot.minHeight = viewport.height;

            pseudoRoot.children.AddRange(roots);

            layoutStack.Push(new LayoutDataSet(pseudoRoot, viewport));

            int retnCount = 0;

            while (layoutStack.Count > 0) {
                LayoutDataSet layoutSet = layoutStack.Pop();
                LayoutData data = layoutSet.data;

                data.layout.Run(viewport, layoutSet, layoutRects);

                if (data.element != null && (data.element.flags & UIElementFlags.RequiresRendering) != 0) {
                    output[retnCount++] = new LayoutResult(data.element.id, layoutSet.result);
                }

                // note: we never need to clear the layoutResults array
                for (int i = 0; i < data.children.Count; i++) {
                    layoutStack.Push(new LayoutDataSet(data.children[i], layoutRects[i]));
                }
            }

            return retnCount;
        }

        public void OnReset() {
            layoutTree.Clear();
        }

        public void OnUpdate() { }

        public void OnDestroy() {
            layoutTree.Clear();
        }

        public void OnElementEnabled(UIElement element) {
            LayoutData data = layoutTree.GetItem(element);
            if (data != null) {
                layoutTree.EnableHierarchy(data);
            }
        }

        public void OnElementDisabled(UIElement element) {
            LayoutData data = layoutTree.GetItem(element);
            if (data != null) {
                layoutTree.EnableHierarchy(data);
            }
        }

        public void OnElementDestroyed(UIElement element) {
            LayoutData data = layoutTree.GetItem(element);
            if (data != null) {
                layoutTree.RemoveHierarchy(data);
            }
        }

        public void SetRectX(UIElement element, UIMeasurement measurement) {
            layoutTree.GetItem(element).staticX = measurement;
        }

        public void SetRectY(UIElement element, UIMeasurement measurement) {
            layoutTree.GetItem(element).staticY = measurement;
        }

        public void SetRectWidth(UIElement element, UIMeasurement measurement) {
            layoutTree.GetItem(element).preferredWidth = measurement;
        }

        public void SetRectHeight(UIElement element, UIMeasurement measurement) {
            layoutTree.GetItem(element).preferredHeight = measurement;
        }

        public void SetRectMinWidth(UIElement element, UIMeasurement measurement) {
            layoutTree.GetItem(element).minWidth = measurement;
        }

        public void SetRectMinHeight(UIElement element, UIMeasurement measurement) {
            layoutTree.GetItem(element).minHeight = measurement;
        }

        public void SetRectMaxWidth(UIElement element, UIMeasurement measurement) {
            layoutTree.GetItem(element).maxWidth = measurement;
        }

        public void SetRectMaxHeight(UIElement element, UIMeasurement measurement) {
            layoutTree.GetItem(element).maxHeight = measurement;
        }

        public void SetGrowthFactor(UIElement element, int factor) {
            layoutTree.GetItem(element).growthFactor = factor;
        }

        public void SetShrinkFactor(UIElement element, int factor) {
            layoutTree.GetItem(element).shrinkFactor = factor;
        }

        public void SetMainAxisAlignment(UIElement element, MainAxisAlignment alignment) {
            layoutTree.GetItem(element).mainAxisAlignment = alignment;
        }

        public void SetCrossAxisAlignment(UIElement element, CrossAxisAlignment alignment) {
            layoutTree.GetItem(element).crossAxisAlignment = alignment;
        }

        public void SetLayoutType(UIElement element, LayoutType layoutType) {
            layoutTree.GetItem(element).layoutType = layoutType;
        }

        public void SetLayoutDirection(UIElement element, LayoutDirection direction) {
            layoutTree.GetItem(element).layoutDirection = direction;
        }

        public void SetLayoutWrap(UIElement element, LayoutWrap wrapMode) {
            layoutTree.GetItem(element).wrapMode = wrapMode;
        }

        public void SetInFlow(UIElement element, bool isInFlow) {
            layoutTree.GetItem(element).isInFlow = isInFlow;
        }

        public void SetMargin(UIElement element, ContentBoxRect rect) {
            layoutTree.GetItem(element).margin = rect;
        }

        public void SetBorder(UIElement element, ContentBoxRect rect) {
            layoutTree.GetItem(element).border = rect;
        }

        public void SetPadding(UIElement element, ContentBoxRect rect) {
            layoutTree.GetItem(element).padding = rect;
        }

        private void SetupLayoutPass(LayoutResult[] layoutResults, LayoutData data) {

            data.children.Clear();

            if (data.parent == null) return;

            data.parent.children.Add(data);

            if (layoutResults.Length < data.parent.children.Count) {
                Array.Resize(ref layoutResults, layoutResults.Length * 2);
            }

        }

    }

}