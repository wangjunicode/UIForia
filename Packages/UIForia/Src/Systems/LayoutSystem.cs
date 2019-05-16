using System;
using System.Collections.Generic;
using SVGX;
using UIForia.Elements;
using UIForia.Extensions;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class LayoutSystem : ILayoutSystem {

        public const int TextLayoutPoolKey = 100;
        public const int ImageLayoutPoolKey = 200;

        public struct ViewRect {

            public readonly UIView view;
            public readonly Rect previousViewport;

            public ViewRect(UIView view, Rect previousViewport) {
                this.view = view;
                this.previousViewport = previousViewport;
            }

        }

        protected readonly IStyleSystem m_StyleSystem;
        protected readonly IntMap<LayoutBox> m_LayoutBoxMap;
        protected readonly LightList<TextLayoutBox> m_TextLayoutBoxes;

        private Size m_ScreenSize;
        private readonly LightList<ViewRect> m_Views;
        private readonly LightList<LayoutBox> m_VisibleBoxList;

        private static readonly IComparer<LayoutBox> comparer = new DepthComparer();
        private readonly Dictionary<int, LayoutBoxPool> layoutBoxPoolMap;
        private readonly LightList<LayoutBox> toLayout = new LightList<LayoutBox>(128);

        public LayoutSystem(IStyleSystem styleSystem) {
            this.m_StyleSystem = styleSystem;
            this.m_LayoutBoxMap = new IntMap<LayoutBox>();
            this.m_Views = new LightList<ViewRect>();
            this.m_VisibleBoxList = new LightList<LayoutBox>();
            this.m_TextLayoutBoxes = new LightList<TextLayoutBox>(64);
            this.m_StyleSystem.onStylePropertyChanged += HandleStylePropertyChanged;
            this.layoutBoxPoolMap = new Dictionary<int, LayoutBoxPool>();

            this.layoutBoxPoolMap[(int) LayoutType.Flex] = new LayoutBoxPool<FlexLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Grid] = new LayoutBoxPool<GridLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Radial] = new LayoutBoxPool<RadialLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Fixed] = new LayoutBoxPool<FixedLayoutBox>();
            this.layoutBoxPoolMap[(int) LayoutType.Flow] = new LayoutBoxPool<FlowLayoutBox>();
            this.layoutBoxPoolMap[TextLayoutPoolKey] = new LayoutBoxPool<TextLayoutBox>();
            this.layoutBoxPoolMap[ImageLayoutPoolKey] = new LayoutBoxPool<ImageLayoutBox>();
        }

        public class DepthComparer : IComparer<LayoutBox> {

            // todo -- profile caching the pointer lookups, either in layout box or as a struct w/ relevant data
            public int Compare(LayoutBox a, LayoutBox b) {
                
                if (a.layer != b.layer) {
                    return a.layer > b.layer ? -1 : 1;
                }

                if (a.viewDepthIdx != b.viewDepthIdx) {
                    return a.viewDepthIdx > b.viewDepthIdx ? -1 : 1;
                }

                if (a.zIndex != b.zIndex) {
                    return a.zIndex > b.zIndex ? -1 : 1;
                }

                return a.traversalIndex > b.traversalIndex ? -1 : 1;
                
//                if (a.element.depth != b.element.depth) {
//                    return a.element.depth > b.element.depth ? -1 : 1;
//                }
//
//                if (a.element.parent != b.element.parent) {
//                    // loop until parents are the same
//                    UIElement ptrA = a.element.parent;
//                    UIElement ptrB = b.element.parent;
//                    
//                    while (ptrA != ptrB) {
//                        ptrA = ptrA.parent;
//                        ptrB = ptrB.parent;
//                    }
//
//                    return ptrA.siblingIndex > ptrB.siblingIndex ? -1 : 1;
//                }
//                
//                return a.element.siblingIndex > b.element.siblingIndex ? -1 : 1;

                
            }

        }

        public void OnReset() {
            m_LayoutBoxMap.Clear();
            m_VisibleBoxList.Clear();
            m_Views.Clear();
        }

        public void OnUpdate() {
            // todo -- should this be a list per-view?
            m_VisibleBoxList.Clear();

            bool forceLayout = false;
            Size screen = new Size(Screen.width, Screen.height);
            if (m_ScreenSize != screen) {
                m_ScreenSize = screen;
                forceLayout = true;
            }

            TextLayoutBox[] textLayouts = m_TextLayoutBoxes.Array;
            for (int i = 0; i < m_TextLayoutBoxes.Count; i++) {
                if (textLayouts[i].TextInfo.LayoutDirty) {
                    textLayouts[i].RequestContentSizeChangeLayout();
                }
            }

            for (int i = 0; i < m_Views.Count; i++) {
                //RunLayout(forceLayout, m_Views[i]);
                RunLayout2(m_Views[i].view);
                m_Views[i] = new ViewRect(m_Views[i].view, m_Views[i].view.Viewport);
            }
        }

        /*
         * for each view
         *     gather list of elements to consider for layout
         *     
         *     if element children exceed width bounds
         *     if overflow y is scroll & scroll layout is push content
         *     run scrollbar vertical layout
         *     run layout again w/ width - scroll bar width
         *     set clip rect x - width to 0 -> scroll bar x (or invert if scroll position inverted)
         *
         *     traverse to find things to layout
         *     run those layouts
         *     compute matrices
         *     compute clip rects
         *     update query grid
         *     copy properites to layout result
         */

        private void CollectLayoutBoxes(UIView view) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            UIElement rootElement = view.rootElement;

            for (int i = 0; i < rootElement.children.Count; i++) {
                stack.Push(rootElement.children[i]);
            }

            int idx = 0;

            toLayout.QuickClear();
            int elementCount = view.GetElementCount();
            toLayout.EnsureCapacity(elementCount);
            LayoutBox[] toLayoutArray = toLayout.Array;

            int viewDepth = view.depth;
            
            while (stack.Count > 0) {
                UIElement currentElement = stack.PopUnchecked();
                
                if (currentElement.isDisabled) {
                    continue;
                }
                
                LayoutBox currentBox = m_LayoutBoxMap.GetOrDefault(currentElement.id);
                currentBox.traversalIndex = idx;
                currentBox.viewDepthIdx = viewDepth;
                
                toLayoutArray[idx++] = currentBox;

                UIElement[] childArray = currentElement.children.Array;
                int childCount = currentElement.children.Count;
                for (int i = childCount - 1; i >= 0; i--) {
                    stack.Push(childArray[i]);
                }
            }

            toLayout.Count = idx;
            LightStack<UIElement>.Release(ref stack);
        }

        public void RunLayout2(UIView view) {
            m_VisibleBoxList.QuickClear();

            UIElement rootElement = view.rootElement;

            LayoutBox rootBox = m_LayoutBoxMap.GetOrDefault(rootElement.id);
            rootBox.element.layoutResult.matrix = SVGXMatrix.identity;
            rootBox.prefWidth = new UIMeasurement(1, UIMeasurementUnit.ViewportWidth);
            rootBox.prefHeight = new UIMeasurement(1, UIMeasurementUnit.ViewportHeight);

            rootBox.allocatedWidth = view.Viewport.width;
            rootBox.allocatedHeight = view.Viewport.height;
            rootBox.actualWidth = rootBox.allocatedWidth;
            rootBox.actualHeight = rootBox.allocatedHeight;

            // todo -- only if changed
            if (view.sizeChanged) {
                rootBox.RunLayout();
                view.sizeChanged = false; // todo - dont do this here
            }

            CollectLayoutBoxes(view);

            LayoutBox[] toLayoutArray = toLayout.Array;
            int toLayoutCount = toLayout.Count;

            for (int i = 0; i < toLayoutCount; i++) {
                LayoutBox box = toLayoutArray[i];

                if (box.IsIgnored) {
                    float currentWidth = box.allocatedWidth;
                    float currentHeight = box.allocatedHeight;
                    box.allocatedWidth = box.GetWidths().clampedSize;
                    box.allocatedHeight = box.GetHeights(box.actualHeight).clampedSize;
                    box.localX = 0;
                    box.localY = 0;
                    if (box.allocatedWidth != currentWidth || box.allocatedHeight != currentHeight) {
                        box.markedForLayout = true;
                    }
                }

                if (box.markedForLayout) {
                    box.RunLayout();
                    box.markedForLayout = false;

                    if (box.style.OverflowY == Overflow.Scroll) {
                        // if needs to scroll
                        // if scroll behavior pushes content

                        // Size vScrollSize = box.style.GetVerticalScrollbar().RunLayout(box.actualSize);
                        // Size hScrollSize = box.style.GetHorizontalScrollbar().RunLayout(box.actualSize);

                        // scrollbar join point?

                        // box.allocatedWidth -= vScrollSize.width;
                        // box.allocatedHeight -= hScrollSize.height;
                        // box.RunLayout();
                    }
                }

                Vector2 scrollOffset = new Vector2();

                LayoutBox parentBox = box.parent;

                scrollOffset.x = (parentBox.actualWidth - parentBox.allocatedWidth) * parentBox.element.scrollOffset.x;
                scrollOffset.y = (parentBox.actualHeight - parentBox.allocatedHeight) * parentBox.element.scrollOffset.y;

                Vector2 localPosition = ResolveLocalPosition(box) - scrollOffset;
                Vector2 localScale = new Vector2(box.transformScaleX, box.transformScaleY);

                LayoutResult layoutResult = box.element.layoutResult;

                Vector2 pivot = box.Pivot;
                SVGXMatrix m;

                if (box.transformRotation != 0) {
                    m = SVGXMatrix.TRS(localPosition, layoutResult.rotation, layoutResult.scale);
                }
                else {
                    m = SVGXMatrix.TranslateScale(localPosition.x, localPosition.y, localScale.x, localScale.y);
                }

                Vector2 offset = new Vector2(box.allocatedWidth * pivot.x, box.allocatedHeight * pivot.y);
                SVGXMatrix parentMatrix = box.parent.element.layoutResult.matrix;
                SVGXMatrix pivotMat = SVGXMatrix.Translation(offset);

                m = pivotMat * m * pivotMat.Inverse();
                m = parentMatrix * m;
                layoutResult.matrix = m;

                layoutResult.localPosition = localPosition;
                layoutResult.ContentRect = box.ContentRect;

                layoutResult.actualSize = new Size(box.actualWidth, box.actualHeight);
                layoutResult.allocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);

                layoutResult.screenPosition = m.position;

                layoutResult.scale.x = localScale.x;
                layoutResult.scale.y = localScale.y;
                layoutResult.rotation = box.transformRotation;
                layoutResult.pivot = pivot;

                layoutResult.borderRadius = new ResolvedBorderRadius(box.BorderRadiusTopLeft, box.BorderRadiusTopRight, box.BorderRadiusBottomRight, box.BorderRadiusBottomLeft);
                layoutResult.border = new OffsetRect(box.BorderTop, box.BorderRight, box.BorderBottom, box.BorderLeft);
                layoutResult.padding = new OffsetRect(box.PaddingTop, box.PaddingRight, box.PaddingBottom, box.PaddingLeft);
            }

            for (int i = 0; i < toLayoutCount; i++) {
                m_VisibleBoxList.Add(toLayoutArray[i]);
            }
            
            for (int i = 0; i < toLayoutCount; i++) {
                LayoutBox box = toLayoutArray[i];
                Rect clipRect = box.parent.clipRect;
                // painter -> no fucking clue
                // rect
                // rounded rect
                // partly rounded rect
                // circle
                // clip-shape(s)
                
            }
            
            // for every layout box
            // clipRect = ViewRect
            // for each child
                // parentPosition & parentAllocatedWidth & Height
            // what clip bounds do we use? if shape is a non rotated rect, can do easily w/ cpu and gpu handle overlap case 
            // clipRectAligned or not
            // if rotated (world space) takes custom code based on shape, clipping gets complicated and graphics has to use polygon clipping or a mask texture
            // if not rotated takes position & scale like normal
            m_VisibleBoxList.Sort(comparer);

            
            // LayoutBox[] boxes = m_VisibleElementList.Array;
            
            //for (int i = 0; i < m_VisibleElementList.Count; i++) {
            //    boxes[i].element.layoutResult.zIndex = i + 1;
            //}

            // compute a clip shape for parent using transform
            // might mean we handle rotated & scaled stuff, get 4 points from box

            // if box has a z index higher than its parent then it is not clipped by the parent? 
        }

        public void RunLayout(bool forceLayout, ViewRect viewRect) {
            Rect rect = viewRect.previousViewport;
            UIView view = viewRect.view;
            Rect viewportRect = view.Viewport;

            LayoutBox realRoot = m_LayoutBoxMap.GetOrDefault(view.rootElement.id);

            realRoot.element.layoutResult.matrix = SVGXMatrix.identity;
            realRoot.prefWidth = new UIMeasurement(1, UIMeasurementUnit.ViewportWidth);
            realRoot.prefHeight = new UIMeasurement(1, UIMeasurementUnit.ViewportHeight);

            realRoot.allocatedWidth = view.Viewport.width;
            realRoot.allocatedHeight = view.Viewport.height;
            realRoot.actualWidth = realRoot.allocatedWidth;
            realRoot.actualHeight = realRoot.allocatedHeight;

            LayoutBox root = m_LayoutBoxMap.GetOrDefault(view.RootElement.id);

            //if (rect != view.Viewport) {
            root.allocatedWidth = Mathf.Min(root.GetWidths().clampedSize, view.Viewport.width);
            root.allocatedHeight = Mathf.Min(root.GetHeights(root.allocatedWidth).clampedSize, view.Viewport.height);
            root.markedForLayout = true;
            //  }

            Stack<UIElement> stack = StackPool<UIElement>.Get();

            // if we don't allow reparenting, could just use a flat sorted list
            // as long as the parent is laid out before the child that should be fine

            UIElement element = view.RootElement;
            LayoutResult layoutResult = element.layoutResult;
            stack.Push(element);

           // m_VisibleBoxList.Add(element);

            if (root.IsIgnored) {
                root.allocatedWidth = root.GetWidths().clampedSize;
                root.allocatedHeight = root.GetHeights(root.allocatedWidth).clampedSize;
            }

            if (forceLayout || root.markedForLayout) {
                root.RunLayout();
                root.markedForLayout = false;
#if DEBUG
                root.layoutCalls++;
#endif
            }

            // actual size should probably be the root containing all children, ignored or not

            layoutResult.actualSize = new Size(root.actualWidth, root.actualHeight);
            layoutResult.allocatedSize = new Size(root.allocatedWidth, root.allocatedHeight);

            layoutResult.ContentRect = root.ContentRect;

            layoutResult.scale = new Vector2(root.transformScaleX, root.transformScaleY);
            layoutResult.localPosition = ResolveLocalPosition(root);
            layoutResult.screenPosition = layoutResult.localPosition;
            layoutResult.rotation = root.transformRotation;
            layoutResult.clipRect = new Rect(0, 0, viewportRect.width, viewportRect.height);

            layoutResult.border = new OffsetRect(
                root.BorderTop,
                root.BorderRight,
                root.BorderBottom,
                root.BorderLeft
            );

            layoutResult.padding = new OffsetRect(
                root.PaddingTop,
                root.PaddingRight,
                root.PaddingBottom,
                root.PaddingLeft
            );

            layoutResult.matrix = SVGXMatrix.TRS(
                new Vector2(root.TransformX, root.TransformY),
                root.transformRotation,
                new Vector2(root.transformScaleX, root.transformScaleY)
            );

            CreateOrDestroyScrollbars(root);

            while (stack.Count > 0) {
                UIElement current = stack.Pop();

                if (!current.isEnabled) {
                    continue;
                }

                if (current.children == null) {
                    continue;
                }

                for (int i = 0; i < current.children.Count; i++) {
                    element = current.children[i];

                    LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);

                    if (!element.isEnabled) {
                        continue;
                    }

                    if (box == null) {
                        stack.Push(element);
                        continue;
                    }

                    if (box.IsIgnored) {
                        float currentWidth = box.allocatedWidth;
                        float currentHeight = box.allocatedHeight;
                        box.allocatedWidth = box.GetWidths().clampedSize;
                        box.allocatedHeight = box.GetHeights(box.actualHeight).clampedSize;
                        box.localX = 0;
                        box.localY = 0;
                        if (box.allocatedWidth != currentWidth || box.allocatedHeight != currentHeight) {
                            box.markedForLayout = true;
                        }
                    }

                    if (forceLayout || box.markedForLayout) {
                        box.RunLayout();
                        box.markedForLayout = false;
                        CreateOrDestroyScrollbars(box);
                    }

                    layoutResult = element.layoutResult;

                    LayoutBox parentBox = box.parent;

                    Vector2 scrollOffset = new Vector2();
                    scrollOffset.x = (parentBox.actualWidth - parentBox.allocatedWidth) * parentBox.element.scrollOffset.x;
                    scrollOffset.y = (parentBox.actualHeight - parentBox.allocatedHeight) * parentBox.element.scrollOffset.y;

                    layoutResult.localPosition = ResolveLocalPosition(box) - scrollOffset;
                    layoutResult.ContentRect = box.ContentRect;
                    layoutResult.actualSize = new Size(box.actualWidth, box.actualHeight);
                    layoutResult.allocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
                    // wrong -- use matrix result
                    layoutResult.screenPosition = parentBox.element.layoutResult.screenPosition + layoutResult.localPosition;
                    layoutResult.scale = new Vector2(box.transformScaleX, box.transformScaleY); // only set if changed

                    // wrong -- use matrix result
                    layoutResult.rotation = parentBox.transformRotation + box.transformRotation; // only set if changed

                    layoutResult.pivot = box.Pivot; // only set if changed

                    layoutResult.borderRadius = new ResolvedBorderRadius(
                        box.BorderRadiusTopLeft,
                        box.BorderRadiusTopRight,
                        box.BorderRadiusBottomRight,
                        box.BorderRadiusBottomLeft
                    );

                    layoutResult.border = new OffsetRect( // only set if changed
                        box.BorderTop,
                        box.BorderRight,
                        box.BorderBottom,
                        box.BorderLeft
                    );

                    layoutResult.padding = new OffsetRect(
                        box.PaddingTop,
                        box.PaddingRight,
                        box.PaddingBottom,
                        box.PaddingLeft
                    );

                    SVGXMatrix m = SVGXMatrix.TRS(layoutResult.localPosition, layoutResult.rotation, layoutResult.scale);
                    Vector2 pivot = box.Pivot;
                    Vector2 offset = new Vector2(layoutResult.allocatedSize.width * pivot.x, layoutResult.allocatedSize.height * pivot.y);
                    SVGXMatrix parentMatrix = box.parent.element.layoutResult.matrix;
                    SVGXMatrix pivotMat = SVGXMatrix.identity.Translate(offset);
                    SVGXMatrix minusPivotMat = pivotMat.Inverse();

                    m = pivotMat * m * minusPivotMat;
                    m = parentMatrix * m;
                    layoutResult.matrix = m;
                    layoutResult.screenPosition = m.position; //parentBox.element.layoutResult.screenPosition + layoutResult.localPosition;

//
//                    // should be able to sort by view
//                    Rect clipRect = new Rect(0, 0, viewportRect.width, viewportRect.height);
//                    UIElement ptr = element.parent;
//                    // find ancestor where layer is higher, might not be our parent
//
//                    // todo -- handle non rect clip shapes: ie circle / ellipse
//
//                    if (ptr != null) {
//                        bool handlesHorizontal = ptr.style.OverflowX != Overflow.Visible;
//                        bool handlesVertical = ptr.style.OverflowY != Overflow.Visible;
//                        if (handlesHorizontal && handlesVertical) {
//                            Rect r = new Rect(ptr.layoutResult.screenPosition, ptr.layoutResult.allocatedSize);
//                            clipRect = clipRect.Intersect(r.Intersect(ptr.layoutResult.clipRect));
//                        }
//                        else if (handlesHorizontal) {
//                            Rect r = new Rect(
//                                ptr.layoutResult.screenPosition.x,
//                                ptr.layoutResult.clipRect.y,
//                                ptr.layoutResult.AllocatedWidth,
//                                ptr.layoutResult.clipRect.height
//                            );
//                            clipRect = r.Intersect(clipRect);
//                        }
//                        else if (handlesVertical) {
//                            Rect r = new Rect(
//                                ptr.layoutResult.clipRect.x,
//                                ptr.layoutResult.screenPosition.y,
//                                ptr.layoutResult.clipRect.width,
//                                ptr.layoutResult.AllocatedHeight
//                            );
//                            clipRect = r.Intersect(clipRect);
//                        }
//                        else {
//                            clipRect = ptr.layoutResult.clipRect;
//                        }
//                    }
//
//                    layoutResult.clipRect = clipRect;
//
//                    Rect intersectedClipRect = layoutResult.clipRect.Intersect(layoutResult.ScreenRect);
//                    CullResult cullResult = CullResult.NotCulled;
//
//                    float clipWAdjustment = 0;
//                    float clipHAdjustment = 0;
//
//                    if (intersectedClipRect.width <= 0 || intersectedClipRect.height <= 0) {
//                        cullResult = CullResult.ClipRectIsZero;
//                    }
//                    else if (layoutResult.actualSize.width * layoutResult.actualSize.height <= 0) {
//                        cullResult = CullResult.ActualSizeZero;
//                    }
//                    else if (layoutResult.allocatedSize.height < layoutResult.actualSize.height) {
//                        clipHAdjustment = 1 - (layoutResult.allocatedSize.height / layoutResult.actualSize.height);
//                        if (clipHAdjustment >= 1) {
//                            cullResult = CullResult.ClipRectIsZero;
//                        }
//                    }
//                    else if (layoutResult.allocatedSize.width < layoutResult.actualSize.width) {
//                        clipWAdjustment = 1 - (layoutResult.allocatedSize.width / layoutResult.actualSize.width);
//                        if (clipWAdjustment >= 1) {
//                            cullResult = CullResult.ClipRectIsZero;
//                        }
//                    }
//
//                    // todo -- can i get rid of clip vector here?
//                    Rect screenRect = layoutResult.ScreenRect;
//                    float clipW = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.xMax, screenRect.xMin, screenRect.xMax)) - clipWAdjustment;
//                    float clipH = Mathf.Clamp01(MathUtil.PercentOfRange(clipRect.yMax, screenRect.yMin, screenRect.yMax)) - clipHAdjustment;
//
//                    if (clipH <= 0 || clipW <= 0) {
//                        cullResult = CullResult.ClipRectIsZero;
//                    }
//
//                    layoutResult.cullState = cullResult;

                    // todo actually use this
                    // if layout result size or position changed -> update the query grid
                    // if (layoutResult.PositionChanged || layoutResult.SizeChanged) {
                    //     UpdateQueryGrid(element, oldScreenRect);
                    // }

                    stack.Push(element);
//                    if (cullResult == CullResult.NotCulled) {
//                    m_VisibleBoxList.Add(element);
//                    }
                }
            }


            m_VisibleBoxList.Sort(comparer);

//            UIElement[] elements = m_VisibleBoxList.Array;
//            for (int i = 0; i < m_VisibleBoxList.Count; i++) {
//                elements[i].layoutResult.zIndex = i + 1;
//            }

            StackPool<UIElement>.Release(stack);
        }

        private void UpdateQueryGrid(UIElement element, Rect oldRect) {
            // todo this should replace the brute force search for querying
            int x = (int) oldRect.x;
            int y = (int) oldRect.y;
            int w = (int) oldRect.width;
            int h = (int) oldRect.height;
            x = (x / 100) * 100;
            y = (y / 100) * 100;
            w = (w / 100) * 100;
            h = (h / 100) * 100;
            int horizontalBlockCount = w / 100;
            int verticalBlockCount = h / 100;
            // note: x and y can be negative!
            // round to nearest 100
            // assume screen divided into n blocks of 100
            // each block addressable by x/y in single int
            // each block has set of members
            // shift as needed
            int start = BitUtil.SetHighLowBits((int) oldRect.x, (int) oldRect.y);
            int end = BitUtil.SetHighLowBits((int) (oldRect.x + oldRect.width), (int) (oldRect.y + oldRect.height));
        }

        private static Vector2 ResolveLocalPosition(LayoutBox box) {
            Vector2 localPosition = Vector2.zero;

            LayoutBehavior layoutBehavior = box.style.LayoutBehavior;
            TransformBehavior transformBehaviorX = box.transformBehaviorX;
            TransformBehavior transformBehaviorY = box.transformBehaviorY;

            switch (layoutBehavior) {
                case LayoutBehavior.TranscludeChildren:
                    localPosition = new Vector2(0, 0);
                    break;

                case LayoutBehavior.Ignored:
                case LayoutBehavior.Normal:

                    switch (transformBehaviorX) {
                        case TransformBehavior.AnchorMinOffset:
                            localPosition.x = box.AnchorLeft + box.TransformX;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.x = box.AnchorRight - box.TransformX - box.actualWidth;
                            break;
                        case TransformBehavior.LayoutOffset:
                            localPosition.x = box.localX + box.TransformX;
                            break;
                        default:
                            localPosition.x = box.localX;
                            break;
                    }

                    switch (transformBehaviorY) {
                        case TransformBehavior.AnchorMinOffset:
                            localPosition.y = box.AnchorTop + box.TransformY;
                            break;
                        case TransformBehavior.AnchorMaxOffset:
                            localPosition.y = box.AnchorBottom - box.TransformY - box.actualHeight;
                            break;
                        case TransformBehavior.LayoutOffset:
                            localPosition.y = box.localY + box.TransformY;
                            break;
                        default:
                            localPosition.y = box.localY;
                            break;
                    }

                    break;
            }

            return localPosition;
        }

        private void CreateOrDestroyScrollbars(LayoutBox box) {
            UIElement element = box.element;

            if (box.actualHeight <= box.allocatedHeight) {
                LayoutResult lr = box.element.layoutResult;
                lr.scrollbarVerticalSize = Size.Unset;
            }
            else {
                Overflow verticalOverflow = box.style.OverflowY;
                if (verticalOverflow == Overflow.Scroll || verticalOverflow == Overflow.ScrollAndAutoHide) {
                    Scrollbar vertical = Application.GetCustomScrollbar(null);

                    Extents childExtents = GetLocalExtents(box.children);
                    float offsetY = (childExtents.min.y < 0) ? -childExtents.min.y / box.allocatedHeight : 0f;
                    element.scrollOffset = new Vector2(element.scrollOffset.x, offsetY);

                    Size originalAllocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);
                    element.layoutResult.actualSize = new Size(box.actualWidth, box.actualHeight);
                    element.layoutResult.allocatedSize = new Size(box.allocatedWidth, box.allocatedHeight);

                    Size verticalScrollbarSize = vertical.RunLayout(element);

                    // this is the push-content case
                    box.allocatedWidth -= (Mathf.Clamp(verticalScrollbarSize.width, 0, box.allocatedWidth));

                    box.RunLayout();

                    element.layoutResult.allocatedSize = originalAllocatedSize;
                    element.layoutResult.scrollbarVerticalSize = verticalScrollbarSize;
                }
            }

//            if (box.actualWidth <= box.allocatedWidth) {
//                if (horizontal != null) {
//                    onDestroyVirtualScrollbar?.Invoke(horizontal);
//                    m_Elements.Remove(horizontal);
//                    m_VirtualElements.Remove(horizontal);
//                    box.horizontalScrollbar = null; // todo -- pool
//                }
//            }
//            else {
//                Overflow horizontalOverflow = box.style.OverflowX;
//                if (horizontal == null && horizontalOverflow == Overflow.Scroll || horizontalOverflow == Overflow.ScrollAndAutoHide) {
//                    horizontal = new VirtualScrollbar(element, ScrollbarOrientation.Horizontal);
//                    // todo -- depth index needs to be set
//
//                    m_Elements.Add(horizontal);
//
//                    m_VirtualElements.Add(horizontal);
//
//                    Extents childExtents = GetLocalExtents(box.children);
//                    float offsetX = (childExtents.min.x < 0) ? -childExtents.min.x / box.allocatedWidth : 0f;
//                    element.scrollOffset = new Vector2(element.scrollOffset.y, offsetX);
//
//                    onCreateVirtualScrollbar?.Invoke(horizontal);
//                    box.horizontalScrollbar = horizontal;
//                }
//            }
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
            CreateLayoutBox(view.rootElement);
            m_Views.Add(new ViewRect(view, new Rect()));
        }

        public void OnViewRemoved(UIView view) {
//            m_Views.Remove(view);
        }

        private void HandleStylePropertyChanged(UIElement element, LightList<StyleProperty> properties) {
            // todo early-out if we haven't had a layout pass for the element yet
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) {
                return;
            }

            bool notifyParent = box.parent != null && (box.style.LayoutBehavior & LayoutBehavior.Ignored) == 0 && box.element.isEnabled;
            bool invalidatePreferredSizeCache = false;
            bool layoutTypeChanged = false;

            for (int i = 0; i < properties.Count; i++) {
                StyleProperty property = properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.PaddingLeft:
                        box.paddingLeft = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.PaddingRight:
                        box.paddingRight = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.PaddingTop:
                        box.paddingTop = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.PaddingBottom:
                        box.paddingBottom = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderLeft:
                        box.borderLeft = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRight:
                        box.borderRight = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderTop:
                        box.borderTop = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderBottom:
                        box.borderBottom = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRadiusTopLeft:
                        box.borderRadiusTopLeft = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRadiusTopRight:
                        box.borderRadiusTopRight = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRadiusBottomLeft:
                        box.borderRadiusBottomLeft = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.BorderRadiusBottomRight:
                        box.borderRadiusBottomRight = property.AsUIFixedLength;
                        break;

                    // todo -- margin should be a fixed measurement probably
                    case StylePropertyId.MarginLeft:
                        box.marginLeft = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MarginRight:
                        box.marginRight = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MarginTop:
                        box.marginTop = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MarginBottom:
                        box.marginBottom = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.TransformPivotX:
                        box.transformPivotX = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.TransformPivotY:
                        box.transformPivotY = property.AsUIFixedLength;
                        break;

                    case StylePropertyId.TransformPositionX:
                        box.transformPositionX = property.AsTransformOffset;
                        break;

                    case StylePropertyId.TransformPositionY:
                        box.transformPositionY = property.AsTransformOffset;
                        break;

                    case StylePropertyId.TransformBehaviorX:
                        box.transformBehaviorX = property.AsTransformBehavior;
                        break;

                    case StylePropertyId.TransformBehaviorY:
                        box.transformBehaviorY = property.AsTransformBehavior;
                        break;

                    case StylePropertyId.TransformRotation:
                        box.transformRotation = property.AsFloat;
                        break;

                    case StylePropertyId.TransformScaleX:
                        box.transformScaleX = property.AsFloat;
                        break;

                    case StylePropertyId.TransformScaleY:
                        box.transformScaleY = property.AsFloat;
                        break;

                    case StylePropertyId.PreferredWidth:
                        box.prefWidth = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.PreferredHeight:
                        box.prefHeight = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MinWidth:
                        box.minWidth = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MinHeight:
                        box.minHeight = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MaxWidth:
                        box.maxWidth = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.MaxHeight:
                        box.maxHeight = property.AsUIMeasurement;
                        break;

                    case StylePropertyId.ZIndex:
                        box.zIndex = property.AsInt;
                        break;

                    case StylePropertyId.Layer:
                        box.layer = property.AsInt;
                        break;
                    
                    case StylePropertyId.LayoutBehavior:
                        // todo -- implement this
                        box.UpdateChildren();

                        break;
                    case StylePropertyId.LayoutType:
                        layoutTypeChanged = true;
                        break;
                }

                if (!invalidatePreferredSizeCache) {
                    switch (property.propertyId) {
                        case StylePropertyId.MinWidth:
                        case StylePropertyId.MaxWidth:
                        case StylePropertyId.PreferredWidth:
                            invalidatePreferredSizeCache = true;
                            break;
                        case StylePropertyId.MinHeight:
                        case StylePropertyId.MaxHeight:
                        case StylePropertyId.PreferredHeight:
                            invalidatePreferredSizeCache = true;
                            break;
                        case StylePropertyId.AnchorTop:
                        case StylePropertyId.AnchorRight:
                        case StylePropertyId.AnchorBottom:
                        case StylePropertyId.AnchorLeft:
                        case StylePropertyId.AnchorTarget:
                            invalidatePreferredSizeCache = true;
                            break;
                    }
                }
            }

            if (layoutTypeChanged) {
                HandleLayoutChanged(element);
            }
            else {
                if (invalidatePreferredSizeCache) {
                    if (notifyParent) {
                        box.RequestContentSizeChangeLayout();
                    }

                    box.InvalidatePreferredSizeCache();
                }

                box.OnStylePropertyChanged(properties);

                if (notifyParent) {
                    box.parent.OnChildStylePropertyChanged(box, properties);
                }
            }
        }

        private void HandleLayoutChanged(UIElement element) {
            LayoutBox box;
            if (!m_LayoutBoxMap.TryGetValue(element.id, out box)) {
                box = CreateLayoutBox(element);
                return;
            }

            LayoutBox parent = box.parent;
            LayoutBox replace = box;

            replace = CreateLayoutBox(element);
            replace.allocatedWidth = box.allocatedWidth;
            replace.allocatedHeight = box.allocatedHeight;
            replace.UpdateChildren();
            replace.parent = parent;
            parent?.UpdateChildren();
            box.Release();
        }

        public struct LayoutBoxPair {

            public UIElement element;
            public LayoutBox parentBox;

            public LayoutBoxPair(UIElement element, LayoutBox parentBox) {
                this.element = element;
                this.parentBox = parentBox;
            }

        }

        public void OnElementEnabled(UIElement element) {
            // none of these boxes should exist for the whole hierarchy, create them

            LightList<LayoutBox> toUpdateList = LightListPool<LayoutBox>.Get();
            LightStack<LayoutBoxPair> stack = LightStack<LayoutBoxPair>.Get();

            if (element.parent != null) {
                stack.Push(new LayoutBoxPair(element, m_LayoutBoxMap.GetOrDefault(element.parent.id)));
            }
            else {
                stack.Push(new LayoutBoxPair(element, null));
            }

            while (stack.Count > 0) {
                LayoutBoxPair current = stack.PopUnchecked();

                if (current.element.isDestroyed || current.element.isDisabled) {
                    continue;
                }

                LayoutBox box = CreateLayoutBox(current.element);
                box.parent = current.parentBox;
                toUpdateList.Add(box);

                int childCount = current.element.children.Count;
                UIElement[] children = current.element.children.Array;

                for (int i = 0; i < childCount; i++) {
                    stack.Push(new LayoutBoxPair(children[i], box));
                }
            }


            int count = toUpdateList.Count;
            LayoutBox[] toUpdate = toUpdateList.Array;

            for (int i = 0; i < count; i++) {
                UpdateChildren(toUpdate[i]);
            }

            if (element.parent != null) {
                LayoutBox ptr = toUpdate[0].parent;
                while (ptr != null) {
                    if (ptr.style.LayoutBehavior != LayoutBehavior.TranscludeChildren) {
                        UpdateChildren(ptr);
                        break;
                    }

                    ptr = ptr.parent;
                }
            }

            LightListPool<LayoutBox>.Release(ref toUpdateList);
            LightStack<LayoutBoxPair>.Release(ref stack);
        }

        public void OnElementDisabled(UIElement element) {
            LightStack<UIElement> stack = LightStack<UIElement>.Get();
            stack.Push(element);
            LayoutBox currentBox = m_LayoutBoxMap.GetOrDefault(element.id);
            LayoutBox parentBox = null;
            if (currentBox != null) {
                parentBox = currentBox.parent;
            }

            while (stack.Count > 0) {
                UIElement current = stack.PopUnchecked();

                if (m_LayoutBoxMap.Remove(current.id, out LayoutBox box)) {
                    box.Release();
                }

                int childCount = current.children.Count;
                UIElement[] children = current.children.Array;
                for (int i = 0; i < childCount; i++) {
                    stack.Push(children[i]);
                }
            }

            LightStack<UIElement>.Release(ref stack);
            if (parentBox != null) {
                UpdateChildren(parentBox);
                parentBox.UpdateChildren();
            }
        }

        private void UpdateChildrenRecursive(UIElement element) {
            if (element.children == null) return;

            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);

            for (int i = 0; i < element.children.Count; i++) {
                UpdateChildrenRecursive(element.children[i]);
            }

            UpdateChildren(box);
            box.UpdateChildren();
        }

        public void OnElementDestroyed(UIElement element) {
            if (m_LayoutBoxMap.Remove(element.id, out LayoutBox box)) {
                if (box.parent != null) {
                    UpdateChildren(box.parent);
                }

                m_VisibleBoxList.Remove(box);
                box.Release();
            }
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        private LayoutBox CreateLayoutBox(UIElement element) {
            LayoutBox retn = null;
            if ((element is UITextElement)) {
                TextLayoutBox textLayout = (TextLayoutBox) layoutBoxPoolMap[TextLayoutPoolKey].Get(element);
                m_TextLayoutBoxes.Add(textLayout);
                retn = textLayout;
            }

            else if ((element is UIImageElement)) {
                retn = layoutBoxPoolMap[ImageLayoutPoolKey].Get(element);
            }

            else {
                switch (element.style.LayoutType) {
                    case LayoutType.Flex:
                        retn = layoutBoxPoolMap[(int) LayoutType.Flex].Get(element);
                        break;

                    case LayoutType.Flow:
                        retn = layoutBoxPoolMap[(int) LayoutType.Flow].Get(element);
                        break;

                    case LayoutType.Fixed:
                        retn = layoutBoxPoolMap[(int) LayoutType.Fixed].Get(element);
                        break;

                    case LayoutType.Grid:
                        retn = layoutBoxPoolMap[(int) LayoutType.Grid].Get(element);
                        break;

                    case LayoutType.Radial:
                        retn = layoutBoxPoolMap[(int) LayoutType.Radial].Get(element);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            retn.UpdateFromStyle();
            m_LayoutBoxMap[element.id] = retn;
            return retn;
        }

        public void OnElementCreated(UIElement element) { }

        private void GetChildBoxes(LayoutBox box, LightList<LayoutBox> list) {
            UIElement element = box.element;
            UIElement[] children = element.children.Array;
            int count = element.children.Count;

            for (int i = 0; i < count; i++) {
                LayoutBox childBox = m_LayoutBoxMap[children[i].id];
                if (childBox == null) {
                    continue;
                }

                LayoutBehavior behavior = childBox.style.LayoutBehavior;
                switch (behavior) {
                    case LayoutBehavior.Unset:
                    case LayoutBehavior.Normal:
                        list.Add(childBox);
                        break;
                    case LayoutBehavior.Ignored:
                        break;
                    case LayoutBehavior.TranscludeChildren:
                        GetChildBoxes(childBox, list);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void UpdateChildren(LayoutBox box) {
            UIElement element = box.element;

            if (box.style.LayoutBehavior == LayoutBehavior.TranscludeChildren) {
                return;
            }

            if (element.children == null || element.children.Count == 0) {
                return;
            }

            LightList<LayoutBox> boxes = LightListPool<LayoutBox>.Get();
            boxes.EnsureCapacity(element.children.Count);

            box.children.Clear();
            GetChildBoxes(box, boxes);
            box.children.AddRange(boxes);
            for (int i = 0; i < boxes.Count; i++) {
                boxes[i].parent = box;
            }

            LightListPool<LayoutBox>.Release(ref boxes);
            box.UpdateChildren();
        }

        public List<UIElement> QueryPoint(Vector2 point, List<UIElement> retn) {
            // todo if point is same as last point or point is off screen, do no work
            if (retn == null) {
                retn = ListPool<UIElement>.Get();
            }

            LayoutBox[] layoutBoxes = m_VisibleBoxList.Array;
            int elementCount = m_VisibleBoxList.Count;
            for (int i = 0; i < elementCount; i++) {
                UIElement element = layoutBoxes[i].element;
                LayoutResult layoutResult = element.layoutResult;

                // todo make this better
                if (element.isDisabled) {
                    continue;
                }

                if (element is IPointerQueryHandler handler) {
                    if (!handler.ContainsPoint(point)) {
                        continue;
                    }
                }
                else if (!layoutResult.ScreenRect.ContainOrOverlap(point)) {
                    continue;
                }

                UIElement ptr = element.parent;
                while (ptr != null) {
                    Vector2 screenPosition = ptr.layoutResult.screenPosition;
                    if (ptr.style.OverflowX != Overflow.Visible) {
                        if (point.x < screenPosition.x || point.x > screenPosition.x + ptr.layoutResult.actualSize.width) {
                            break;
                        }
                    }

                    if (ptr.style.OverflowY != Overflow.Visible) {
                        if (point.y < screenPosition.y || point.y > screenPosition.y + ptr.layoutResult.actualSize.height) {
                            break;
                        }
                    }

                    ptr = ptr.parent;
                }

                if (ptr == null) {
                    retn.Add(element);
                }
            }

            return retn;
        }

        public OffsetRect GetPaddingRect(UIElement element) {
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) return new OffsetRect();
            return new OffsetRect(
                box.PaddingTop,
                box.PaddingRight,
                box.PaddingBottom,
                box.PaddingLeft
            );
        }

        public OffsetRect GetMarginRect(UIElement element) {
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) return new OffsetRect();
            return new OffsetRect(
                box.GetMarginTop(box.actualWidth),
                box.GetMarginRight(),
                box.GetMarginBottom(box.actualWidth),
                box.GetMarginLeft()
            );
        }

        public OffsetRect GetBorderRect(UIElement element) {
            LayoutBox box = m_LayoutBoxMap.GetOrDefault(element.id);
            if (box == null) return new OffsetRect();
            return new OffsetRect(
                box.BorderTop,
                box.BorderRight,
                box.BorderBottom,
                box.BorderLeft
            );
        }

        public LayoutBox GetBoxForElement(UIElement itemElement) {
            return m_LayoutBoxMap.GetOrDefault(itemElement.id);
        }

        public LightList<UIElement> GetVisibleElements(LightList<UIElement> retn = null) {
            if (retn == null) {
                retn = new LightList<UIElement>(m_VisibleBoxList.Count);
            }
            else {
                retn.EnsureCapacity(m_VisibleBoxList.Count);
            }

            UIElement[] elements = retn.Array;
            LayoutBox[] boxes = m_VisibleBoxList.Array;
            for (int i = 0; i < m_VisibleBoxList.Count; i++) {
                elements[i] = boxes[i].element;
            }
            
            retn.Count = m_VisibleBoxList.Count;
            return retn;
        }

        private static Extents GetLocalExtents(List<LayoutBox> children) {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                if (child.element.isDisabled) continue;

                Rect rect = child.element.layoutResult.LocalRect;
                Vector2 localPosition = new Vector2(rect.x, rect.y);

                if (localPosition.x < min.x) {
                    min.x = localPosition.x;
                }

                if (localPosition.y < min.y) {
                    min.y = localPosition.y;
                }

                if (localPosition.x + rect.width > max.x) {
                    max.x = localPosition.x + rect.width;
                }

                if (localPosition.y + rect.height > max.y) {
                    max.y = localPosition.y + rect.height;
                }
            }

            return new Extents(min, max);
        }

    }

}