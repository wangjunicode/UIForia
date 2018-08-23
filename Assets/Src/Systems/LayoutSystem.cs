﻿using System;
using System.Collections.Generic;
using Rendering;
using Src.Layout;
using UnityEngine;

namespace Src.Systems {

    public class LayoutSystem : ISystem {

        private readonly Rect[] layoutRects;
        private readonly SkipTree<LayoutData> layoutTree;
        private readonly Stack<LayoutDataSet> layoutStack;
        private readonly IStyleSystem styleSystem;
        private readonly Dictionary<int, LayoutData> layoutDataMap;

        public LayoutSystem(IStyleSystem styleSystem) {
            this.styleSystem = styleSystem;
            this.layoutRects = new Rect[16];
            this.layoutTree = new SkipTree<LayoutData>();
            this.layoutStack = new Stack<LayoutDataSet>();
            this.layoutDataMap = new Dictionary<int, LayoutData>();
            this.styleSystem.onRectChanged += HandleRectChanged;
            this.styleSystem.onLayoutChanged += HandleLayoutChanged;
            this.styleSystem.onBorderChanged += HandleBorderChanged;
            this.styleSystem.onMarginChanged += HandleMarginChanged;
            this.styleSystem.onPaddingChanged += HandlePaddingChanged;
            this.styleSystem.onConstraintChanged += HandleConstraintChanged;
        }

        private void HandleRectChanged(int elementId, LayoutRect rect) {
            LayoutData data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.rect = rect;
            }
        }

        private void HandleConstraintChanged(int elementId, LayoutConstraints constraints) {
            LayoutData data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.constraints = constraints;
            }
        }

        private void HandlePaddingChanged(int elementId, ContentBoxRect padding) {
            LayoutData data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.padding = padding;
            }
        }

        private void HandleMarginChanged(int elementId, ContentBoxRect margin) {
            LayoutData data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.margin = margin;
            }
        }

        private void HandleBorderChanged(int elementId, ContentBoxRect border) {
            LayoutData data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.border = border;
            }
        }

        private void HandleLayoutChanged(int elementId, LayoutParameters parameters) {
            LayoutData data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.parameters = parameters;
            }
        }

        public void OnInitialize() {
            IReadOnlyList<UIStyleSet> styles = styleSystem.GetAllStyles();
            for (int i = 0; i < styles.Count; i++) {
                LayoutData data;
                UIStyleSet styleSet = styles[i];
                if (layoutDataMap.TryGetValue(styles[i].elementId, out data)) {
                    data.border = styleSet.border;
                    data.padding = styleSet.padding;
                    data.margin = styleSet.margin;
                    data.parameters = styleSet.layout;
                    data.constraints = styleSet.constraints;
                    data.rect = styleSet.rect;
                }
            }
        }

        private void CreateOrUpdateElement(UIElement element) { }

        public void OnElementCreated(UIElementCreationData elementData) {
            // todo -- if instance is layout-able

            if ((elementData.element.flags & UIElementFlags.RequiresLayout) == 0) {
                return;
            }

            if ((elementData.element.flags & UIElementFlags.TextElement) != 0) {
                UITextElement textElement = (UITextElement) elementData.element;
                textElement.onSizeChanged += HandleTextSizeChanged;
            }

            LayoutData data = new LayoutData(elementData.element);
            if (data.element.style != null) {
                data.border = data.element.style.border;
                data.padding = data.element.style.padding;
                data.margin = data.element.style.margin;
                data.parameters = data.element.style.layout;
                data.constraints = data.element.style.constraints;
                data.rect = data.element.style.rect;
            }
            else {
                data.parameters.type = LayoutType.Flex;
                data.parameters.crossAxisAlignment = CrossAxisAlignment.Stretch;
                data.parameters.direction = LayoutDirection.Row;
            }

            layoutTree.AddItem(data);
            layoutDataMap[data.element.id] = data;
        }

        public int RunLayout(Rect viewport, ref LayoutResult[] output) {
            layoutTree.TraversePreOrderWithCallback(output, SetupLayoutPass);

            LayoutData[] roots = layoutTree.GetRootItems();
            
            // temp
//            roots[0].parameters.mainAxisAlignment = MainAxisAlignment.Start;
            
            layoutStack.Push(new LayoutDataSet(roots[0], viewport));

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
            layoutDataMap.Clear();
            layoutTree.Clear();
        }

        public void OnUpdate() { }

        public void OnDestroy() {
            this.styleSystem.onRectChanged -= HandleRectChanged;
            this.styleSystem.onLayoutChanged -= HandleLayoutChanged;
            this.styleSystem.onBorderChanged -= HandleBorderChanged;
            this.styleSystem.onMarginChanged -= HandleMarginChanged;
            this.styleSystem.onPaddingChanged -= HandlePaddingChanged;
            this.styleSystem.onConstraintChanged -= HandleConstraintChanged;
            layoutDataMap.Clear();
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
                // todo -- recurse this hierarchy and do a proper destroy call on each
                layoutTree.RemoveHierarchy(data);
                if ((data.element.flags & UIElementFlags.TextElement) != 0) {
                    UITextElement textElement = (UITextElement) data.element;
                    textElement.onSizeChanged -= HandleTextSizeChanged;
                }
            }
        }

        private void HandleTextSizeChanged(UIElement element, Vector2 size) {
            LayoutData layoutData;
            if (layoutDataMap.TryGetValue(element.id, out layoutData)) {
                layoutData.rect = new LayoutRect(layoutData.rect.x, layoutData.rect.y, size.x, size.y);
            }
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