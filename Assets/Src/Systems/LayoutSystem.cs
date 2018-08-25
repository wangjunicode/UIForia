using System;
using System.Collections.Generic;
using Rendering;
using Src.Layout;
using UnityEngine;

namespace Src.Systems {

    public class LayoutSystem : ILayoutSystem {

        private readonly Rect[] layoutRects;
        private readonly IStyleSystem styleSystem;
        private readonly IElementRegistry registry;
        private readonly SkipTree<LayoutData> layoutTree;
        private readonly Stack<LayoutDataSet> layoutStack;
        private readonly Dictionary<int, LayoutData> layoutDataMap;
        
        // todo -- use simple array for this w/ unique ids -> indices on elements
        private readonly Dictionary<int, Rect> layoutResultMap;
        
        private readonly FlexLayout flexLayout;
        private readonly FlowLayout flowLayout;
        private readonly FixedLayout fixedLayout;

        private bool isInitialized;
        private int rectCount;
        private LayoutResult[] rects;
        private Rect viewport;

        /*
         * It is possible to have a scheme where element ids can also contain an integer index into lists
         * To do this we need to maintain a list of available ids a-la bitsquid packed list
         */
        public LayoutSystem(ITextSizeCalculator textSizeCalculator, IElementRegistry registry, IStyleSystem styleSystem) {
            this.styleSystem = styleSystem;
            this.registry = registry;
            this.layoutRects = new Rect[16];
            this.layoutTree = new SkipTree<LayoutData>();
            this.layoutStack = new Stack<LayoutDataSet>();
            this.layoutDataMap = new Dictionary<int, LayoutData>();
            this.layoutResultMap = new Dictionary<int, Rect>();
            
            this.styleSystem.onRectChanged += HandleRectChanged;
            this.styleSystem.onLayoutChanged += HandleLayoutChanged;
            this.styleSystem.onBorderChanged += HandleBorderChanged;
            this.styleSystem.onMarginChanged += HandleMarginChanged;
            this.styleSystem.onPaddingChanged += HandlePaddingChanged;
            this.styleSystem.onConstraintChanged += HandleConstraintChanged;

            this.flexLayout = new FlexLayout(textSizeCalculator);
            this.rects = new LayoutResult[16];
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
                data.SetPadding(padding);
            }
        }

        private void HandleMarginChanged(int elementId, ContentBoxRect margin) {
            LayoutData data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.SetMargin(margin);
            }
        }

        private void HandleBorderChanged(int elementId, ContentBoxRect border) {
            LayoutData data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.SetBorder(border);
            }
        }

        private void HandleLayoutChanged(int elementId, LayoutParameters parameters) {
            LayoutData data;
            if (layoutDataMap.TryGetValue(elementId, out data)) {
                data.parameters = parameters;
                data.SetLayout(GetLayoutInstance(parameters.type));
            }
        }

        private void HandleTextChanged(UIElement element, string text) {
            LayoutData layoutData;
            if (layoutDataMap.TryGetValue(element.id, out layoutData)) {
                layoutData.SetTextContent(text);
            }
        }

        public void OnInitialize() {
            isInitialized = true;
            IReadOnlyList<UIStyleSet> styles = styleSystem.GetAllStyles();
            for (int i = 0; i < styles.Count; i++) {
                LayoutData data;
                UIStyleSet styleSet = styles[i];
                UIElement element = registry.GetElement(styleSet.elementId);
                UITextElement textElement;
                if (!layoutDataMap.TryGetValue(styles[i].elementId, out data)) {
                    data = new LayoutData(element);
                    layoutTree.AddItem(data);
                    layoutDataMap[data.element.id] = data;
                    if ((element.flags & UIElementFlags.TextElement) != 0) {
                        textElement = (UITextElement) element;
                        textElement.onTextChanged += HandleTextChanged;
                    }

                }
                data.UpdateFromStyle();
                data.SetLayout(GetLayoutInstance(styleSet.layoutType));
                textElement = element as UITextElement;
                if (textElement != null) {
                    data.SetTextContent(textElement.GetText());
                }
            }
        }

        public void OnElementCreated(UIElementCreationData elementData) {
            // todo -- if instance is layout-able
            if (!isInitialized || elementData.element.style == null) return;

            if ((elementData.element.flags & UIElementFlags.TextElement) != 0) {
                UITextElement textElement = (UITextElement) elementData.element;
                textElement.onTextChanged += HandleTextChanged;
            }

            LayoutData data = new LayoutData(elementData.element);
            data.UpdateFromStyle();
            data.SetLayout(GetLayoutInstance(elementData.element.style.layoutType));

            layoutTree.AddItem(data);
            layoutDataMap[data.element.id] = data;
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

            rectCount = 0;
            layoutResultMap.Clear();
            // todo change this not to return a new array copy
            // todo this can be made better w/ new skip tree traversal methods
            LayoutData[] roots = layoutTree.GetRootItems();

            if (roots.Length == 0) {
                return;
            }

            layoutTree.TraversePreOrder(rects, SetupLayoutPass);

            layoutStack.Push(new LayoutDataSet(roots[0], viewport));

            while (layoutStack.Count > 0) {
                LayoutDataSet layoutSet = layoutStack.Pop();
                LayoutData data = layoutSet.data;

                data.layout.Run(viewport, layoutSet, layoutRects);

                layoutResultMap[data.element.id] = layoutSet.result;
                
                if (data.element != null && (data.element.flags & UIElementFlags.RequiresRendering) != 0) {
                    rects[rectCount++] = new LayoutResult(data.element.id, layoutSet.result);
                }

                // note: we never need to clear the layoutResults array
                for (int i = 0; i < data.children.Count; i++) {
                    layoutStack.Push(new LayoutDataSet(data.children[i], layoutRects[i]));
                }
            }
        }

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
                    textElement.onTextChanged -= HandleTextChanged;
                }
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