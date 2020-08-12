using System;
using UIForia.Attributes;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.UIInput;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace UIForia.Elements {

    // shared unmanaged struct so the layout system can access and modify these values
    internal struct ScrollValues {

        public float scrollX;
        public float scrollY;
        public float contentWidth;
        public float contentHeight;
        public bool isOverflowingX;
        public bool isOverflowingY;
        public float actualWidth;
        public float actualHeight;

    }

    [Template(TemplateType.Internal, "Elements/ScrollView.xml")]
    public unsafe class ScrollView : UIElement {

        public float scrollSpeedY = 48f;
        public float scrollSpeedX = 16f;
        public float trackSize = 10f;

        public bool disableOverflowX;
        public bool disableOverflowY;

        public bool disableAutoScroll = false;

        public bool verticalScrollingEnabled => !disableOverflowY && isOverflowingY;
        public bool horizontalScrollingEnabled => !disableOverflowX && isOverflowingX;

        private Size previousChildrenSize;

        public bool isOverflowingX {
            get => scrollValues != null && scrollValues->isOverflowingX;
        }

        public bool isOverflowingY {
            get => scrollValues != null && scrollValues->isOverflowingY;
        }

        internal float scrollDeltaX;
        internal float scrollDeltaY;

        private float elapsedTotalTime;

        // this lets the unmanaged layoutbox share values with the element and still be bursted
        internal ScrollValues* scrollValues;

        private float fromScrollY;
        private float toScrollY;
        private bool isScrollingY;

        private float fromScrollX;
        private float toScrollX;
        private bool isScrollingX;

        private float accumulatedScrollSpeedY;
        private float accumulatedScrollSpeedX;

        internal float scrollPercentageX {
            get {
                if (scrollValues != null) return scrollValues->scrollX;
                scrollValues = TypedUnsafe.Malloc<ScrollValues>(Allocator.Persistent);
                *scrollValues = default;

                return scrollValues->scrollX;
            }
            set {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                if (scrollValues == null) {
                    scrollValues = TypedUnsafe.Malloc<ScrollValues>(Allocator.Persistent);
                    *scrollValues = default;
                }

                scrollValues->scrollX = value;
            }
        }

        internal float scrollPercentageY {
            get {
                if (scrollValues != null) return scrollValues->scrollY;
                scrollValues = TypedUnsafe.Malloc<ScrollValues>(Allocator.Persistent);
                *scrollValues = default;

                return scrollValues->scrollY;
            }
            set {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                if (scrollValues == null) {
                    scrollValues = TypedUnsafe.Malloc<ScrollValues>(Allocator.Persistent);
                    *scrollValues = default;
                }

                scrollValues->scrollY = value;
            }
        }

        internal ScrollValues* GetScrollValues() {
            if (scrollValues != null) return scrollValues;
            scrollValues = TypedUnsafe.Malloc<ScrollValues>(Allocator.Persistent);
            *scrollValues = default;

            return scrollValues;
        }

        public override void OnUpdate() {
            if (isScrollingY) {
                elapsedTotalTime += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(Easing.Interpolate(elapsedTotalTime / 0.500f, EasingFunction.CubicEaseOut));
                scrollPercentageY = Mathf.Lerp(fromScrollY, toScrollY, t);
                isScrollingY = t < 1;
            }
            else if (isScrollingX) {
                elapsedTotalTime += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(Easing.Interpolate(elapsedTotalTime / 0.500f, EasingFunction.CubicEaseOut));
                scrollPercentageX = Mathf.Lerp(fromScrollX, toScrollX, t);
                isScrollingX = t < 1;
            }

       

            // if (!firstChild.isEnabled) {
            //     // isOverflowingX = false;
            //     // isOverflowingY = false;
            // }
            // else {
            //     Size currentChildrenSize = new Size(firstChild.layoutResult.actualSize.width, firstChild.layoutResult.allocatedSize.height);
            //
            //     // isOverflowingX = currentChildrenSize.width > layoutResult.actualSize.width;
            //     // isOverflowingY = currentChildrenSize.height > layoutResult.actualSize.height;
            //
            //     if (!disableAutoScroll && currentChildrenSize != previousChildrenSize) {
            //         ScrollToHorizontalPercent(0);
            //         ScrollToVerticalPercent(0);
            //     }
            //
            //     previousChildrenSize = currentChildrenSize;
            // }
        }

        public override void OnDisable() {
            scrollDeltaX = 0;
            scrollDeltaY = 0;
        }

        [OnMouseWheel]
        public void OnMouseWheel(MouseInputEvent evt) {
            if (verticalScrollingEnabled) {
                float actualContentHeight = scrollValues->contentHeight;
                float visibleContentHeight = scrollValues->actualHeight;
                if (!isScrollingY || (int) Mathf.Sign(evt.ScrollDelta.y) != (int) Mathf.Sign(fromScrollY - toScrollY)) {
                    accumulatedScrollSpeedY = scrollSpeedY;
                }
                else {
                    accumulatedScrollSpeedY *= 1.2f;
                }

                scrollDeltaY = -evt.ScrollDelta.y * accumulatedScrollSpeedY / (actualContentHeight - visibleContentHeight);
                fromScrollY = scrollPercentageY;
                if (scrollDeltaY != 0) {
                    toScrollY = Mathf.Clamp01(fromScrollY + scrollDeltaY);
                    if (fromScrollY != toScrollY) {
                        evt.StopPropagation();
                        elapsedTotalTime = 0;
                        isScrollingY = true;
                    }
                }
            }

            if (horizontalScrollingEnabled) {
                float actualContentWidth = scrollValues->contentWidth;
                float visibleContentWidth = scrollValues->actualWidth;
                if (!isScrollingX || (int) Mathf.Sign(-evt.ScrollDelta.x) != (int) Mathf.Sign(fromScrollX - toScrollX)) {
                    accumulatedScrollSpeedX = scrollSpeedX;
                }
                else {
                    accumulatedScrollSpeedX *= 1.2f;
                }

                scrollDeltaX = evt.ScrollDelta.x * accumulatedScrollSpeedX / (actualContentWidth - visibleContentWidth);
                if (scrollDeltaX != 0) {
                    fromScrollX = scrollPercentageX;
                    toScrollX = Mathf.Clamp01(fromScrollX + scrollDeltaX);
                    if (fromScrollX != toScrollX) {
                        evt.StopPropagation();
                        elapsedTotalTime = 0;
                        isScrollingX = true;
                    }
                }
            }
        }

        public void OnClickVertical(MouseInputEvent evt) {
            // float contentAreaHeight = layoutResult.ContentAreaHeight;
            // float contentHeight = firstChild.layoutResult.actualSize.height;
            // float paddingBorderStart = layoutResult.VerticalPaddingBorderStart;
            // float y = evt.MousePosition.y - layoutResult.screenPosition.y - paddingBorderStart;
            //
            // if (contentHeight == 0) return;
            //
            // float handleHeight = (contentAreaHeight / contentHeight) * contentAreaHeight;
            //
            // float handlePosition = (paddingBorderStart + contentAreaHeight - handleHeight) * scrollPercentageY;
            //
            // float pageSize = evt.element.layoutResult.allocatedSize.height / contentHeight;
            //
            // if (y < handlePosition) {
            //     pageSize = -pageSize;
            // }
            //
            // ScrollToVerticalPercent(scrollPercentageY + pageSize);

            evt.StopPropagation();
        }

        public void OnClickHorizontal(MouseInputEvent evt) {
            // float x = evt.MousePosition.x - layoutResult.screenPosition.x;
            //
            // float contentAreaWidth = layoutResult.ContentAreaWidth;
            // float contentWidth = firstChild.layoutResult.actualSize.width;
            //
            // if (contentWidth == 0) return;
            //
            // float handleWidth = (contentAreaWidth / contentWidth) * contentAreaWidth;
            //
            // float handlePosition = (contentAreaWidth - handleWidth) * scrollPercentageX;
            //
            // float pageSize = evt.element.layoutResult.allocatedSize.width / contentWidth;
            //
            // if (x < handlePosition) {
            //     pageSize = -pageSize;
            // }
            //
            // ScrollToHorizontalPercent(scrollPercentageX + pageSize);

            evt.StopPropagation();
        }

        [OnDragCreate(EventPhase.Capture)]
        public DragEvent OnMiddleMouseDrag(MouseInputEvent evt) {
            if (!evt.IsMouseMiddleDown) {
                return null;
            }

            Vector2 baseOffset = new Vector2();
            ScrollbarOrientation orientation = 0;
            Vector2 baseScroll = default;

            if (horizontalScrollingEnabled) {
                baseOffset.x = evt.MousePosition.x - layoutResult.screenPosition.x;
                orientation |= ScrollbarOrientation.Horizontal;
                baseScroll.x = scrollValues->scrollX;
            }

            if (verticalScrollingEnabled) {
                baseOffset.y = evt.MousePosition.y - layoutResult.screenPosition.y; // maybe also - padding / border?
                orientation |= ScrollbarOrientation.Vertical;
                baseScroll.y = scrollValues->scrollY;
            }

            return new ScrollbarDragEvent(orientation, baseOffset, baseScroll, this);
        }

        public virtual DragEvent OnCreateVerticalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            float baseOffset = evt.MousePosition.y - (evt.element.layoutResult.screenPosition.y);
            return new ScrollbarDragEvent(ScrollbarOrientation.Vertical, new Vector2(0, baseOffset), default, this);
        }

        public virtual DragEvent OnCreateHorizontalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            float baseOffset = evt.MousePosition.x - evt.element.layoutResult.screenPosition.x;
            return new ScrollbarDragEvent(ScrollbarOrientation.Horizontal, new Vector2(baseOffset, 0), default, this);
        }

        public void ScrollToVerticalPercent(float percentage) {
            scrollDeltaY = 0;
            scrollPercentageY = Mathf.Clamp01(percentage);
        }

        public void ScrollToHorizontalPercent(float percentage) {
            scrollDeltaX = 0;
            scrollPercentageX = Mathf.Clamp01(percentage);
        }

        public class ScrollbarDragEvent : DragEvent {

            public readonly Vector2 baseOffset;
            public readonly Vector2 baseScroll;
            public readonly ScrollView scrollView;
            public readonly ScrollbarOrientation orientation;

            public ScrollbarDragEvent(ScrollbarOrientation orientation, Vector2 baseOffset, Vector2 baseScroll, ScrollView scrollView) {
                this.orientation = orientation;
                this.baseOffset = baseOffset;
                this.scrollView = scrollView;
                this.baseScroll = baseScroll;
            }

            public override void Update() {
                ScrollValues* scrollValues = scrollView.GetScrollValues();
                if ((orientation & ScrollbarOrientation.Vertical) != 0) {
                    float height = scrollValues->actualHeight;

                    float handleHeight = (scrollValues->actualHeight / scrollValues->contentHeight) * scrollValues->actualHeight;

                    height -= handleHeight;

                    float y = MousePosition.y - (scrollView.layoutResult.screenPosition.y + scrollView.layoutResult.VerticalPaddingBorderStart) - baseOffset.y;

                    if (height == 0) {
                        scrollView.ScrollToVerticalPercent(0);
                    }
                    else {
                        scrollView.ScrollToVerticalPercent(baseScroll.y + y / height);
                    }
                }

                if ((orientation & ScrollbarOrientation.Horizontal) != 0) {
                    float width = scrollValues->actualWidth;

                    float handleWidth = (scrollValues->actualWidth / scrollValues->contentWidth) * scrollValues->actualWidth;

                    width -= handleWidth;

                    float x = MousePosition.x - (scrollView.layoutResult.screenPosition.x + scrollView.layoutResult.HorizontalPaddingBorderStart) - baseOffset.x;

                    if (width == 0) {
                        scrollView.ScrollToHorizontalPercent(0);
                    }
                    else {
                        scrollView.ScrollToHorizontalPercent(baseScroll.x + x / width);
                    }
                }
            }

        }

        // public float ScrollOffsetX => -(firstChild.layoutResult.alignedPosition.x - layoutResult.HorizontalPaddingBorderStart);
        public float ScrollOffsetY {
            get {
                UIElement child = GetFirstChild().GetFirstChild(); // first for children element, second for first actual child, todo -- really we want to find the first non ignored child
                if (child == null) {
                    return layoutResult.VerticalPaddingBorderStart;
                }

                return child.layoutResult.alignedPosition.y - layoutResult.VerticalPaddingBorderStart;
            } 
        }

        internal void ScrollElementIntoView(UIElement element, float crawlPositionX, float crawlPositionY) {
            // float scrollOffsetX = ScrollOffsetX;
            // float localPositionX = crawlPositionX - layoutResult.HorizontalPaddingBorderStart;
            //
            // float elementWidth = element.layoutResult.ActualWidth;
            // float elementRight = localPositionX + scrollOffsetX + elementWidth;
            //
            // float childrenWidth = firstChild.layoutResult.ActualWidth;
            // float contentWidth = layoutResult.ContentAreaWidth;
            //
            // if (localPositionX < 0) {
            //     // scrolls to the left edge of the element
            //     ScrollToHorizontalPercent((localPositionX + scrollOffsetX) / (childrenWidth - contentWidth));
            // }
            // else if (elementRight - scrollOffsetX > contentWidth) {
            //     // scrolls to the right edge but keeps the element at the right edge of the scrollView
            //     ScrollToHorizontalPercent(((elementRight - contentWidth) / (childrenWidth - contentWidth)));
            // }
            //
            // float scrollOffsetY = ScrollOffsetY;
            // float localPositionY = crawlPositionY - layoutResult.VerticalPaddingBorderStart;
            //
            // float elementHeight = element.layoutResult.ActualHeight;
            // float elementBottom = localPositionY + scrollOffsetY + elementHeight;
            //
            // float childrenHeight = firstChild.layoutResult.ActualHeight;
            // float contentHeight = layoutResult.ContentAreaHeight;
            //
            // if (localPositionY < 0) {
            //     // scrolls up to the upper edge of the element
            //     ScrollToVerticalPercent((localPositionY + scrollOffsetY) / (childrenHeight - contentHeight));
            // }
            // else if (elementBottom - scrollOffsetY > contentHeight) {
            //     // scrolls down but keeps the element at the lower edge of the scrollView
            //     ScrollToVerticalPercent(((elementBottom - contentHeight) / (childrenHeight - contentHeight)));
            // }
        }

        public override void OnDestroy() {
            if (scrollValues != null) {
                TypedUnsafe.Dispose(scrollValues, Allocator.Persistent);
            }

            scrollValues = default;
        }

    }

}