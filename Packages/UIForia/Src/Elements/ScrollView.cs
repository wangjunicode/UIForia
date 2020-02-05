using UIForia.Attributes;
using UIForia.Layout;
using UIForia.UIInput;
using UnityEngine;

namespace UIForia.Elements {

    [Template(TemplateType.Internal, "Elements/ScrollView.xml")]
    public class ScrollView : UIElement {

        public float scrollSpeed = 1f;
        public float fadeTime = 2f;
        public float trackSize = 10f;

        public bool disableOverflowX;
        public bool disableOverflowY;

        public bool verticalScrollingEnabled => !disableOverflowY && isOverflowingY;
        public bool horizontalScrollingEnabled => !disableOverflowX && isOverflowingX;

        public float fadeTarget;

        private Size previousChildrenSize;

        public float scrollPercentageX;
        public float scrollPercentageY;

        public float horizontalScrollPosition;
        public float verticalScrollPosition;

        public bool isOverflowingX { get; internal set; }
        public bool isOverflowingY { get; internal set; }

        internal float scrollDeltaX;
        internal float scrollDeltaY;
        internal int xDirection;
        internal int yDirection;

        internal UIElement verticalHandle;
        internal UIElement horizontalHandle;

        public override void OnEnable() {
            verticalHandle = children[2];
            horizontalHandle = children[4];
        }

        public override void OnUpdate() {
            scrollDeltaX *= 0.1f;
            scrollDeltaY *= 0.1f;

            if (scrollDeltaX < 0.0001) scrollDeltaX = 0;
            if (scrollDeltaY < 0.0001) scrollDeltaY = 0;

            scrollPercentageX = Mathf.Clamp01(scrollPercentageX + (Time.unscaledDeltaTime * scrollDeltaX * xDirection));
            scrollPercentageY = Mathf.Clamp01(scrollPercentageY + (Time.unscaledDeltaTime * scrollDeltaY * yDirection));

            if (!children[0].isEnabled) {
                isOverflowingX = false;
                isOverflowingY = false;
            }
            else {
                isOverflowingX = children[0].layoutResult.allocatedSize.width > layoutResult.allocatedSize.width;
                isOverflowingY = children[0].layoutResult.allocatedSize.height > layoutResult.allocatedSize.height;
            }
        }


        public override void OnDisable() {
            scrollDeltaX = 0;
            scrollDeltaY = 0;
        }

        [OnMouseWheel]
        public void OnMouseWheel(MouseInputEvent evt) {
            if (verticalScrollingEnabled) {
                scrollDeltaY = -evt.ScrollDelta.y * scrollSpeed;
                yDirection = (int) Mathf.Sign(scrollDeltaY);
                scrollDeltaY = Mathf.Abs(scrollDeltaY);
                scrollPercentageY = Mathf.Clamp01(scrollPercentageY + (Time.unscaledDeltaTime * scrollDeltaY * yDirection));
                evt.StopPropagation();
            }

            if (horizontalScrollingEnabled) {
                scrollDeltaX = evt.ScrollDelta.x * scrollSpeed;
                xDirection = (int) Mathf.Sign(scrollDeltaX);
                scrollDeltaX = Mathf.Abs(scrollDeltaX);
                scrollPercentageX = Mathf.Clamp01(scrollPercentageX + (Time.unscaledDeltaTime * scrollDeltaX * xDirection));
                evt.StopPropagation();
            }
        }

        public void OnClickVertical(MouseInputEvent evt) {
            float y = evt.MousePosition.y - layoutResult.screenPosition.y;

            float contentAreaHeight = layoutResult.ContentAreaHeight;
            float contentHeight = children[0].layoutResult.actualSize.height;

            if (contentHeight == 0) return;

            float handleHeight = (contentAreaHeight / contentHeight) * contentAreaHeight;

            float handlePosition = (contentAreaHeight - handleHeight) * scrollPercentageY;

            float pageSize = evt.element.layoutResult.allocatedSize.height / contentHeight;

            if (y < handlePosition) {
                pageSize = -pageSize;
            }

            ScrollToVerticalPercent(scrollPercentageY + pageSize);

            evt.StopPropagation();
        }

        public void OnClickHorizontal(MouseInputEvent evt) {
            float x = evt.MousePosition.x - layoutResult.screenPosition.x;

            float contentAreaWidth = layoutResult.ContentAreaWidth;
            float contentWidth = children[0].layoutResult.actualSize.width;

            if (contentWidth == 0) return;

            float handleWidth = (contentAreaWidth / contentWidth) * contentAreaWidth;

            float handlePosition = (contentAreaWidth - handleWidth) * scrollPercentageX;

            float pageSize = evt.element.layoutResult.allocatedSize.width / contentWidth;

            if (x < handlePosition) {
                pageSize = -pageSize;
            }

            ScrollToHorizontalPercent(scrollPercentageX + pageSize);

            evt.StopPropagation();
        }


        // public void OnHoverHorizontal(MouseInputEvent evt) {
        //     lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
        // }
        //
        // public void OnHoverVertical(MouseInputEvent evt) {
        //     lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
        // }

        [OnDragCreate(EventPhase.Capture)]
        public DragEvent OnMiddleMouseDrag(MouseInputEvent evt) {
            if (!evt.IsMouseMiddleDown) {
                return null;
            }

            Vector2 baseOffset = new Vector2();
            ScrollbarOrientation orientation = 0;

            if (horizontalScrollingEnabled) {
                baseOffset.x = evt.MousePosition.x - horizontalHandle.layoutResult.screenPosition.x;
                orientation |= ScrollbarOrientation.Horizontal;
            }

            if (verticalScrollingEnabled) {
                baseOffset.y = evt.MousePosition.y - verticalHandle.layoutResult.screenPosition.y;
                orientation |= ScrollbarOrientation.Vertical;
            }

            return new ScrollbarDragEvent(orientation, baseOffset, this);
        }

        public virtual DragEvent OnCreateVerticalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            float baseOffset = evt.MousePosition.y - evt.element.layoutResult.screenPosition.y;
            return new ScrollbarDragEvent(ScrollbarOrientation.Vertical, new Vector2(0, baseOffset), this);
        }

        public virtual DragEvent OnCreateHorizontalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            float baseOffset = evt.MousePosition.x - evt.element.layoutResult.screenPosition.x;
            return new ScrollbarDragEvent(ScrollbarOrientation.Horizontal, new Vector2(baseOffset, 0), this);
        }

        public override void HandleUIEvent(UIEvent evt) {
            if (evt is UIScrollEvent scrollEvent) {
                if (scrollEvent.ScrollDestinationX >= 0) {
                    ScrollToHorizontalPercent(scrollEvent.ScrollDestinationX);
                }

                if (scrollEvent.ScrollDestinationY >= 0) {
                    ScrollToVerticalPercent(scrollEvent.ScrollDestinationY);
                }
            }
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
            public readonly ScrollView scrollView;
            public readonly ScrollbarOrientation orientation;

            public ScrollbarDragEvent(ScrollbarOrientation orientation, Vector2 baseOffset, ScrollView scrollView) {
                this.orientation = orientation;
                this.baseOffset = baseOffset;
                this.scrollView = scrollView;
            }

            public override void Update() {
                if ((orientation & ScrollbarOrientation.Vertical) != 0) {
                    float height = scrollView.layoutResult.ContentAreaHeight;

                    height -= scrollView.verticalHandle.layoutResult.actualSize.height;

                    float y = Mathf.Clamp(MousePosition.y - scrollView.layoutResult.screenPosition.y - baseOffset.y, 0, height);

                    if (height == 0) {
                        scrollView.ScrollToVerticalPercent(0);
                    }
                    else {
                        scrollView.ScrollToVerticalPercent(y / height);
                    }
                }

                if ((orientation & ScrollbarOrientation.Horizontal) != 0) {
                    float width = scrollView.layoutResult.ContentAreaWidth;

                    width -= scrollView.horizontalHandle.layoutResult.actualSize.width;

                    float y = Mathf.Clamp(MousePosition.x - scrollView.layoutResult.screenPosition.x - baseOffset.x, 0, width);

                    if (width == 0) {
                        scrollView.ScrollToHorizontalPercent(0);
                    }
                    else {
                        scrollView.ScrollToHorizontalPercent(y / width);
                    }
                }
            }

        }

    }

}