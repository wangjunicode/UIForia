//using System;
//using System.Collections.Generic;
//using Rendering;
//using Src.Elements;
//using Src.Layout;
//using Src.Util;
//using UnityEngine;
//using GridLayout = Src.Layout.GridLayout;
//using TreeChangeType = Src.SkipTree<UIElement>.TreeChangeType;
//
//namespace Src.Systems {
//
//    public class LayoutSystem : ILayoutSystem {
//
//        private readonly FlexLayout flexLayout;
//        private readonly FlowLayout flowLayout;
//        private readonly GridLayout gridLayout;
//        private readonly FixedLayout fixedLayout;
//        private readonly IStyleSystem styleSystem;
//        private readonly SkipTree<LayoutNode> layoutTree;
//        private readonly List<VirtualScrollbar> m_VirtualScrollBars;
//        private readonly List<ClipRect> m_ClipRects;
//        private readonly List<UIElement> m_LayoutResults;
//
//        private Rect viewport;
//        private bool isReady;
//
//        public event Action<VirtualScrollbar> onCreateVirtualScrollbar;
//        public event Action<VirtualScrollbar> onDestroyVirtualScrollbar;
//
//        private SkipTree<LayoutNode>.TreeNode tree;
//        private bool m_TreeUpdateRequired;
//
//        public LayoutSystem(ITextSizeCalculator textSizeCalculator, IStyleSystem styleSystem) {
//            this.styleSystem = styleSystem;
//            this.layoutTree = new SkipTree<LayoutNode>();
//
//            this.flexLayout = new FlexLayout(textSizeCalculator);
//            this.fixedLayout = new FixedLayout(textSizeCalculator);
//            this.gridLayout = new GridLayout(textSizeCalculator);
//            this.m_LayoutResults = new List<UIElement>(128);
//            this.m_ClipRects = new List<ClipRect>();
//            this.m_VirtualScrollBars = new List<VirtualScrollbar>();
//            this.layoutTree.onItemParentChanged += HandleTreeParentChanged;
//            this.m_TreeUpdateRequired = true;
//        }
//
//        private struct ClipRect {
//
//            public readonly int depth;
//            public readonly Rect rect;
//            public readonly int elementId;
//
//            public ClipRect(int elementId, int depth, Rect rect) {
//                this.elementId = elementId;
//                this.depth = depth;
//                this.rect = rect;
//            }
//
//        }
//
//        public void SetViewportRect(Rect viewport) {
//            this.viewport = viewport;
//        }
//
//        public void OnInitialize() {
//            this.styleSystem.onLayoutChanged += HandleLayoutChanged;
//            this.styleSystem.onTextContentChanged += HandleTextChanged;
//            this.styleSystem.onRectChanged += HandleRectChanged;
//            this.styleSystem.onBorderChanged += HandleContentBoxChanged;
//            this.styleSystem.onPaddingChanged += HandleContentBoxChanged;
//            this.styleSystem.onMarginChanged += HandleContentBoxChanged;
//            this.styleSystem.onConstraintChanged += HandleConstraintChanged;
//            this.styleSystem.onFontPropertyChanged += HandleFontPropertyChanged;
//            this.styleSystem.onOverflowPropertyChanged += HandleOverflowPropertyChanged;
//        }
//
//        public void OnElementCreated(UIElement element) { }
//
//        public void OnElementMoved(UIElement element, int newIndex, int oldIndex) { }
//
//        public void OnReady() {
//            isReady = true;
//            layoutTree.TraversePreOrder(this, (self, node) => { node.UpdateData(self); });
//        }
//
//        public void OnElementCreatedFromTemplate(MetaData elementData) {
//            m_TreeUpdateRequired = true;
//            if ((elementData.element.flags & UIElementFlags.RequiresLayout) != 0) {
//                LayoutNode node = new LayoutNode(elementData.element);
//                layoutTree.AddItem(node);
//
//                if (isReady) {
//                    node.UpdateData(this);
//                }
//            }
//
//            for (int i = 0; i < elementData.children.Count; i++) {
//                OnElementCreatedFromTemplate(elementData.children[i]);
//            }
//        }
//
//        public UILayout GetLayoutInstance(LayoutType layoutType) {
//            switch (layoutType) {
//                case LayoutType.Flex:
//                    return flexLayout;
//                case LayoutType.Fixed:
//                    return fixedLayout;
//                case LayoutType.Grid:
//                    return gridLayout;
//            }
//
//            throw new NotImplementedException();
//        }
//
//        public void OnUpdate() {
//            if (m_TreeUpdateRequired) {
//                // todo -- recycle tree to avoid allocation of arrays
//                tree = layoutTree.GetTraversableTree().children[0];
//                m_TreeUpdateRequired = false;
//            }
//
//            m_LayoutResults.Clear();
//            m_ClipRects.Clear();
//
//            tree.item.element.layoutResult = new LayoutResult(tree.item.element) {
//                localPosition = new Vector2(),
//                screenPosition = new Vector2(viewport.x, viewport.y),
//                size = new Size(viewport.width, viewport.height)
//            };
//
//            Stack<SkipTree<LayoutNode>.TreeNode> nodeStack = StackPool<SkipTree<LayoutNode>.TreeNode>.Get();
//            Stack<Rect> clipStack = StackPool<Rect>.Get();
//
//            nodeStack.Push(tree);
//
//            while (nodeStack.Count > 0) {
//                SkipTree<LayoutNode>.TreeNode current = nodeStack.Pop();
//                UIElement element = current.item.element;
//                LayoutNode currentNode = current.item;
//
//                if (element.isDisabled || (element.flags & UIElementFlags.Destroyed) != 0) {
//                    continue;
//                }
//
//                m_LayoutResults.Add(element);
//
//                List<Rect> results = currentNode.layout.Run(viewport, current.item);
//
//                Extents childExtents = GetLocalExtents(results);
//
//                float contentWidth = childExtents.max.x - childExtents.min.x;
//                float contentHeight = childExtents.max.y - childExtents.min.y;
//
//                // layout only returns a list of local rects
//
//                Rect clipRect;
//                
//                if (HandleClipping(currentNode, childExtents, out clipRect)) {
//                    m_ClipRects.Add(new ClipRect(element.id, element.depth, clipRect));
//                }
//
//                LayoutResult result = element.layoutResult;
//                result.contentSize = childExtents.Size;
//
//                Vector2 scrollOffset = new Vector2(
//                    element.scrollOffset.x * (contentWidth - result.size.width),
//                    element.scrollOffset.y * (contentHeight - result.size.height)
//                );
//
//                Vector2 childStart = result.screenPosition + new Vector2(
//                                         current.item.contentStartOffsetX,
//                                         current.item.contentStartOffsetY
//                                     );
//
//                element.layoutResult = result;
//
//                for (int i = 0; i < current.children.Length; i++) {
//                    nodeStack.Push(current.children[i]);
//
//                    Rect childRect = results[i];
//                    UIElement child = current.children[i].item.element;
//
//                    Vector2 localPosition = new Vector2(childRect.x, childRect.y) - scrollOffset;
//
//                    child.layoutResult = new LayoutResult(child) {
//                        localPosition = localPosition,
//                        screenPosition = childStart + localPosition,
//                        size = new Size(childRect.width, childRect.height)
//                    };
//                }
//
//                ListPool<Rect>.Release(results);
//            }
//
//            m_ClipRects.Sort((a, b) => a.depth < b.depth ? 1 : -1);
//
//            StackPool<SkipTree<LayoutNode>.TreeNode>.Release(nodeStack);
//            StackPool<Rect>.Release(clipStack);
//        }
//
//
//        public List<LayoutResult> GetLayoutResults(List<LayoutResult> retn = null) {
//            if (retn == null) {
//                retn = ListPool<LayoutResult>.Get();
//            }
//
//            for (int i = 0; i < m_LayoutResults.Count; i++) {
//                retn.Add(m_LayoutResults[i].layoutResult);
//            }
//
//            return retn;
//        }
//
//
//        // todo this needs to be replaced with a quad tree eventually
//        public List<UIElement> QueryPoint(Vector2 point, List<UIElement> retn = null) {
//            if (retn == null) {
//                retn = ListPool<UIElement>.Get();
//            }
//
//            for (int i = 0; i < m_LayoutResults.Count; i++) {
//                LayoutResult layoutResult = m_LayoutResults[i].layoutResult;
//                UIElement element = m_LayoutResults[i];
//
//                if (!layoutResult.ScreenRect.Contains(point)) {
//                    continue;
//                }
//
//                bool shouldAdd = true;
//                for (int j = 0; j < m_ClipRects.Count; j++) {
//                    // for each clip rect
//                    // if clip rect is higher or equal depth -> break;
//                    // if clip rect is not in ancestry -> break;
//                    // if clip rect does not contain the point -> should add = false; break;
//
//                    ClipRect clipRect = m_ClipRects[j];
//                    if (clipRect.depth >= element.depth) {
//                        break;
//                    }
//
//                    if (IsDescendantOf(element, clipRect.elementId)) {
//                        if (!clipRect.rect.Contains(point)) {
//                            shouldAdd = false;
//                            break;
//                        }
//                    }
//                }
//
//                if (shouldAdd) {
//                    retn.Add(m_LayoutResults[i]);
//                }
//            }
//
//            for (int i = 0; i < m_VirtualScrollBars.Count; i++) {
//                VirtualScrollbar scrollbar = m_VirtualScrollBars[i];
//                if (scrollbar.layoutResult.ScreenRect.Contains(point)) {
//                    bool shouldAdd = true;
//                    for (int j = 0; j < m_ClipRects.Count; j++) {
//                        // for each clip rect
//                        // if clip rect is higher or equal depth -> break;
//                        // if clip rect is not in ancestry -> break;
//                        // if clip rect does not contain the point -> should add = false; break;
//
//                        ClipRect clipRect = m_ClipRects[j];
//                        if (clipRect.depth >= scrollbar.targetElement.depth) {
//                            break;
//                        }
//
//                        if (IsDescendantOf(scrollbar.targetElement, clipRect.elementId)) {
//                            if (!clipRect.rect.Contains(point)) {
//                                shouldAdd = false;
//                                break;
//                            }
//                        }
//                    }
//
//                    if (shouldAdd) {
//                        retn.Add(m_LayoutResults[i]);
//                    }
//                }
//            }
//
//            return retn;
//        }
//
//        public void OnReset() {
//            m_TreeUpdateRequired = true;
//            OnDestroy();
//        }
//
//        public void OnDestroy() {
//            this.styleSystem.onFontPropertyChanged -= HandleFontPropertyChanged;
//            this.styleSystem.onLayoutChanged -= HandleLayoutChanged;
//            this.styleSystem.onTextContentChanged -= HandleTextChanged;
//            this.styleSystem.onRectChanged -= HandleRectChanged;
//            this.styleSystem.onBorderChanged -= HandleContentBoxChanged;
//            this.styleSystem.onPaddingChanged -= HandleContentBoxChanged;
//            this.styleSystem.onMarginChanged -= HandleContentBoxChanged;
//            this.styleSystem.onConstraintChanged -= HandleConstraintChanged;
//            layoutTree.Clear();
//        }
//
//        public void OnElementEnabled(UIElement element) { }
//
//        public void OnElementDisabled(UIElement element) { }
//
//        public void OnElementDestroyed(UIElement element) {
//            m_TreeUpdateRequired = true;
//            layoutTree.RemoveHierarchy(element);
//        }
//
//        public void OnElementShown(UIElement element) { }
//
//        public void OnElementHidden(UIElement element) { }
//
//        public void OnElementParentChanged(UIElement element, UIElement oldParent, UIElement newParent) {
//            layoutTree.UpdateItemParent(element);
//            m_TreeUpdateRequired = true;
//        }
//
//        private void HandleFontPropertyChanged(UIElement element, TextStyle textStyle) {
//            LayoutNode node = layoutTree.GetItem(element);
//            node?.UpdateTextMeasurements();
//        }
//
//        private void HandleLayoutChanged(UIElement element, LayoutParameters parameters) {
//            LayoutNode node = layoutTree.GetItem(element);
//            if (node == null) return;
//            node.layout = GetLayoutInstance(parameters.type);
//        }
//
//        private void HandleTextChanged(UIElement element, string text) {
//            LayoutNode node = layoutTree.GetItem(element);
//            node?.SetTextContent(text);
//        }
//
//        private static void HandleTreeParentChanged(LayoutNode child, LayoutNode newParent, LayoutNode oldParent) {
//            oldParent?.children.Remove(child);
//            newParent?.children.Add(child);
//        }
//
//        private void HandleConstraintChanged(UIElement element, LayoutConstraints constraints) {
//            LayoutNode node = layoutTree.GetItem(element);
//            node?.UpdateData(this);
//        }
//
//        private void HandleContentBoxChanged(UIElement element, ContentBoxRect rect) {
//            LayoutNode node = layoutTree.GetItem(element);
//            node?.UpdateData(this);
//        }
//
//        private void HandleRectChanged(UIElement element, Dimensions rect) {
//            LayoutNode node = layoutTree.GetItem(element);
//            node?.UpdateData(this);
//        }
//
//        private void HandleOverflowPropertyChanged(UIElement element) {
//            
//        }
//
//        private bool HandleClipping(LayoutNode currentNode, Extents childExtents, out Rect clipRect) {
//            UIElement element = currentNode.element;
//            bool isValidClipRect = false;
//            clipRect = new Rect();
//            LayoutResult layoutResult = currentNode.element.layoutResult;
//
//            if (element.style.HandlesOverflowX) {
//                bool isOverflowing = childExtents.min.x < 0 || childExtents.max.x > layoutResult.width;
//
//                if (isOverflowing) {
//                    float contentWidth = element.layoutResult.contentSize.width;
//                    VirtualScrollbar horizontal;
//                    if (currentNode.horizontalScrollbar == null) {
//                        horizontal = new VirtualScrollbar(element, ScrollbarOrientation.Horizontal);
//                        m_VirtualScrollBars.Add(horizontal);
//                        currentNode.horizontalScrollbar = horizontal;
//                        float offsetX = (childExtents.min.x < 0) ? -childExtents.min.x / contentWidth : 0f;
//                        element.scrollOffset = new Vector2(offsetX, element.scrollOffset.y);
//                        onCreateVirtualScrollbar?.Invoke(horizontal);
//                    }
//                    else {
//                        horizontal = currentNode.horizontalScrollbar;
//                    }
//
//                    Rect trackRect = horizontal.GetTrackRect();
//                    horizontal.layoutResult = new LayoutResult(horizontal) {
//                        screenPosition = new Vector2(trackRect.x, trackRect.y),
//                        size = new Size(layoutResult.width, 5f),
//                        contentSize = new Size(contentWidth, 5f),
//                    };
//
//                    clipRect.x = layoutResult.screenPosition.x;
//                    clipRect.width = layoutResult.width;
//                    isValidClipRect = true;
//                }
//                else if (currentNode.horizontalScrollbar != null) {
//                    onDestroyVirtualScrollbar?.Invoke(currentNode.horizontalScrollbar);
//                    currentNode.horizontalScrollbar = null;
//                }
//            }
//
//            if (element.style.HandlesOverflowY) {
//                bool isOverflowing = childExtents.min.y < 0 || childExtents.max.y > layoutResult.height;
//
//                if (isOverflowing) {
//                    VirtualScrollbar vertical;
//                    float contentHeight = element.layoutResult.contentSize.height;
//                    if (currentNode.verticalScrollbar == null) {
//                        vertical = new VirtualScrollbar(element, ScrollbarOrientation.Vertical);
//                        currentNode.verticalScrollbar = vertical;
//                        m_VirtualScrollBars.Add(vertical);
//
//                        // initialize scroll offset
//                        float offsetY = (childExtents.min.y < 0) ? -childExtents.min.y / contentHeight : 0f;
//                        element.scrollOffset = new Vector2(element.scrollOffset.x, offsetY);
//                        onCreateVirtualScrollbar?.Invoke(vertical);
//                    }
//                    else {
//                        vertical = currentNode.verticalScrollbar;
//                    }
//
//                    Rect trackRect = vertical.GetTrackRect();
//                    vertical.layoutResult = new LayoutResult(vertical) {
//                        screenPosition = new Vector2(trackRect.x, trackRect.y),
//                        size = new Size(5f, layoutResult.height),
//                        contentSize = new Size(5f, layoutResult.contentHeight)
//                    };
//                    clipRect.y = layoutResult.screenPosition.y;
//                    clipRect.height = layoutResult.height;
//                    isValidClipRect = true;
//                }
//                else if (currentNode.verticalScrollbar != null) {
//                    onDestroyVirtualScrollbar?.Invoke(currentNode.verticalScrollbar);
//                    currentNode.verticalScrollbar = null;
//                }
//            }
//
//            return isValidClipRect;
//        }
//
//        private static bool IsDescendantOf(UIElement element, int id) {
//            UIElement ptr = element;
//            while (ptr.parent != null) {
//                if (ptr.parent.id == id) {
//                    return true;
//                }
//
//                ptr = ptr.parent;
//            }
//
//            return false;
//        }
//
//        private static Extents GetLocalExtents(List<Rect> rects) {
//            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
//            Vector2 max = new Vector2(float.MinValue, float.MinValue);
//
//            for (int i = 0; i < rects.Count; i++) {
//                Rect rect = rects[i];
//                Vector2 localPosition = new Vector2(rect.x, rect.y);
//
//                if (localPosition.x < min.x) {
//                    min.x = localPosition.x;
//                }
//
//                if (localPosition.y < min.y) {
//                    min.y = localPosition.y;
//                }
//
//                if (localPosition.x + rect.width > max.x) {
//                    max.x = localPosition.x + rect.width;
//                }
//
//                if (localPosition.y + rect.height > max.y) {
//                    max.y = localPosition.y + rect.height;
//                }
//            }
//
//            return new Extents(min, max);
//        }
//
//    }
//
//}