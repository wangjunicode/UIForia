using UIForia.Attributes;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEngine;

namespace UIForia.Elements {

    [Template(TemplateType.Internal, "Elements/ScrollView.xml")]
    public class ScrollView : UIElement {

        public UIElement verticalHandle { get; protected set; }
        public UIElement verticalTrack { get; protected set; }

        public UIElement horizontalHandle { get; protected set; }
        public UIElement horizontalTrack { get; protected set; }

        public float scrollSpeed = 50f;
        public float fadeTime = 2f;

        public bool disableOverflowX;
        public bool disableOverflowY;

        public float fadeTarget; 

        protected float lastScrollVerticalTimestamp;
        protected float lastScrollHorizontalTimestamp;

        protected UIElement childrenElement;
        // todo -- without layout system integration this is an overlay scroll bar only

        private Size overflowSize;

        private Size previousChildrenSize;

        private float scrollPercentageX;
        private float scrollPercentageY;

        public override void OnEnable() {
            childrenElement = FindById("scroll-root");
            verticalHandle = FindById("scroll-handle-vertical");
            verticalTrack = FindById("scroll-track-vertical");
            horizontalHandle = FindById("scroll-handle-horizontal");
            horizontalTrack = FindById("scroll-track-horizontal");
        }

        public override void OnUpdate() {
            float contentWidth = layoutResult.ContentWidth;
            float contentHeight = layoutResult.ContentHeight;
          
            overflowSize = childrenElement.layoutResult.ComputeOverflowSize();

            if (previousChildrenSize != default && (int) previousChildrenSize.height > (int) overflowSize.height) {
                ScrollToVerticalPercent(0);
            }
            if (previousChildrenSize != default && (int) previousChildrenSize.width > (int) overflowSize.width) {
                ScrollToHorizontalPercent(0);
            }

            previousChildrenSize = overflowSize;

            if (disableOverflowX || overflowSize.width <= contentWidth) {
                horizontalTrack.SetEnabled(false);
            }
            else {
                horizontalTrack.SetEnabled(true);
                float width = (contentWidth / overflowSize.width) * contentWidth;
                float opacity = 1 + fadeTarget - Mathf.Clamp01(Easing.Interpolate((Time.realtimeSinceStartup - lastScrollHorizontalTimestamp) / fadeTime, EasingFunction.CubicEaseInOut));
                horizontalHandle.style.SetPreferredWidth(width, StyleState.Normal);
                horizontalTrack.style.SetOpacity(opacity, StyleState.Normal);
            }

            if (disableOverflowY || overflowSize.height <= contentHeight) {
                verticalTrack.SetEnabled(false);
            }
            else {
                verticalTrack.SetEnabled(true);
                // todo fix bug: settings preferred height does not immediately update the height
                float height = (contentHeight / overflowSize.height) * contentHeight;
                float opacity = 1 + fadeTarget - Mathf.Clamp01(Easing.Interpolate((Time.realtimeSinceStartup - lastScrollVerticalTimestamp) / fadeTime, EasingFunction.CubicEaseInOut));
                verticalHandle.style.SetPreferredHeight(height, StyleState.Normal);
                verticalTrack.style.SetOpacity(opacity, StyleState.Normal);
            }
        }

        public void OnClickVertical(MouseInputEvent evt) {
            ScrollPageTowardsY(evt.MousePosition.y);
            evt.StopPropagation();
        }

        public void OnClickHorizontal(MouseInputEvent evt) {
            ScrollPageTowardsX(evt.MousePosition.x);
            evt.StopPropagation();
        }
        
        [OnMouseWheel]
        public void OnMouseWheel(MouseInputEvent evt) {
            if (verticalTrack.isEnabled) {
                float max = GetMaxHeight();
                float offset = (verticalHandle.layoutResult.screenPosition.y - verticalTrack.layoutResult.screenPosition.y) + (scrollSpeed * -evt.ScrollDelta.y);
                ScrollToVerticalPercent(offset / max);
                evt.StopPropagation();
            }

            if (horizontalTrack.isEnabled) {
                float max = GetMaxWidth();
                float offset = (horizontalHandle.layoutResult.screenPosition.x - horizontalTrack.layoutResult.screenPosition.x) - (scrollSpeed * evt.ScrollDelta.x);
                ScrollToHorizontalPercent(offset / max);
                evt.StopPropagation();
            }
        }

        public void OnHoverHorizontal(MouseInputEvent evt) {
            lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
        }

        public void OnHoverVertical(MouseInputEvent evt) {
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
        }

        private void ScrollPageTowardsY(float y) {
            float pageSize = verticalTrack.layoutResult.allocatedSize.height / overflowSize.height;
            float handleTop = verticalHandle.layoutResult.screenPosition.y;

            if (y < handleTop) {
                pageSize = -pageSize;
            }

            if (handleTop == 0) {
                ScrollToVerticalPercent(0);
            }
            else {
                ScrollToVerticalPercent(handleTop / GetMaxHeight() + pageSize);
            }
        }

        private void ScrollPageTowardsX(float x) {
            float pageSize = horizontalTrack.layoutResult.allocatedSize.width / overflowSize.width;
            float handleLeft = horizontalHandle.layoutResult.screenPosition.x;

            if (x < handleLeft) {
                pageSize = -pageSize;
            }

            if (handleLeft == 0) {
                ScrollToVerticalPercent(0);
            }
            else {
                ScrollToHorizontalPercent(handleLeft / GetMaxWidth() + pageSize);
            }
        }

        [OnDragCreate(EventPhase.Capture)]
        public DragEvent OnMiddleMouseDrag(MouseInputEvent evt) {
            if (!evt.IsMouseMiddleDown) {
                return null;
            }

            Vector2 baseOffset = new Vector2();
            ScrollbarOrientation orientation = 0;

            if (!disableOverflowX && overflowSize.width > layoutResult.ContentWidth) {
                lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
                baseOffset.x = evt.MousePosition.x - horizontalHandle.layoutResult.screenPosition.x;
                orientation |= ScrollbarOrientation.Horizontal;
            }

            if (!disableOverflowY && overflowSize.height > layoutResult.ContentHeight) {
                lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
                baseOffset.y = evt.MousePosition.y - verticalHandle.layoutResult.screenPosition.y;
                orientation |= ScrollbarOrientation.Vertical;
            }

            return new ScrollbarDragEvent(orientation, baseOffset, this);
        }

        public virtual DragEvent OnCreateVerticalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
            float handlePosition = verticalHandle.layoutResult.screenPosition.y;
            float baseOffset = evt.LeftMouseDownPosition.y - handlePosition;
            return new ScrollbarDragEvent(ScrollbarOrientation.Vertical, new Vector2(0, baseOffset), this);
        }

        public virtual DragEvent OnCreateHorizontalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
            float handlePosition = horizontalHandle.layoutResult.screenPosition.x;
            float baseOffset = evt.LeftMouseDownPosition.x - handlePosition;
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
            if (!verticalTrack.isEnabled) {
                return;
            }

            scrollPercentageY = Mathf.Clamp01(percentage);
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
            float scrollPixels = overflowSize.height - layoutResult.ContentHeight;

            verticalHandle.style.SetAlignmentOffsetY(new OffsetMeasurement(-scrollPercentageY, OffsetMeasurementUnit.Percent), StyleState.Normal);
            verticalHandle.style.SetAlignmentOriginY(new OffsetMeasurement(scrollPercentageY, OffsetMeasurementUnit.Percent), StyleState.Normal);
            childrenElement.style.SetAlignmentOriginY(new OffsetMeasurement(-scrollPercentageY * scrollPixels), StyleState.Normal);
        }

        public void ScrollToHorizontalPercent(float percentage) {
            if (!horizontalTrack.isEnabled) {
                return;
            }

            lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
            scrollPercentageX = Mathf.Clamp01(percentage);
            float scrollPixels = overflowSize.width - layoutResult.ContentWidth;

            horizontalHandle.style.SetAlignmentOffsetX(new OffsetMeasurement(-scrollPercentageX, OffsetMeasurementUnit.Percent), StyleState.Normal);
            horizontalHandle.style.SetAlignmentOriginX(new OffsetMeasurement(scrollPercentageX, OffsetMeasurementUnit.Percent), StyleState.Normal);
            childrenElement.style.SetAlignmentOriginX(new OffsetMeasurement(-scrollPercentageX * scrollPixels), StyleState.Normal);
        }
        
        public void ScrollElementIntoView(UIElement element) {

            float localPositionY = element.layoutResult.localPosition.y;
            UIElement ptr = element.parent;
            while (ptr != childrenElement) {
                if (ptr == null) {
                    // maybe throw an exception. cannot scroll something into view that's not a child
                    return;
                }

                localPositionY += ptr.layoutResult.localPosition.y;
                ptr = ptr.parent;
            }

            float elementHeight = element.layoutResult.ActualHeight;
            float elementBottom = localPositionY + elementHeight;

            float trackHeight = layoutResult.ContentHeight;
            float minY = layoutResult.localPosition.y;
            if (elementBottom + childrenElement.style.AlignmentOriginY.value <= trackHeight
                && localPositionY + childrenElement.style.AlignmentOriginY.value >= 0) {
                return;
            }

            if (localPositionY < 0) {
                // scrolls up to the upper edge of the element
                ScrollToVerticalPercent((localPositionY - minY) / (trackHeight - overflowSize.height));
            }
            else {
                // scrolls down but keeps the element at the lower edge of the scrollView
                ScrollToVerticalPercent((elementBottom - trackHeight - minY) / (overflowSize.height - trackHeight));
            }
        }

        private float GetMaxHeight() {
            float trackRectHeight = verticalTrack.layoutResult.actualSize.height;
            float handleHeight = verticalHandle.layoutResult.actualSize.height;
            return trackRectHeight - handleHeight;
        }

        private float GetMaxWidth() {
            float trackRectWidth = horizontalTrack.layoutResult.actualSize.width;
            float handleWidth = horizontalHandle.layoutResult.actualSize.width;
            return trackRectWidth - handleWidth;
        }

        public class ScrollbarDragEvent : DragEvent {

            public readonly Vector2 baseOffset;
            public readonly ScrollView scrollView;
            public readonly ScrollbarOrientation orientation;

            public ScrollbarDragEvent(ScrollbarOrientation orientation, Vector2 baseOffset, ScrollView scrollView) : base(scrollView) {
                this.orientation = orientation;
                this.baseOffset = baseOffset;
                this.scrollView = scrollView;
            }

            public override void Update() {
                if ((orientation & ScrollbarOrientation.Vertical) != 0) {
                    float max = scrollView.GetMaxHeight();
                    float offset = Mathf.Clamp(MousePosition.y - scrollView.verticalTrack.layoutResult.screenPosition.y - baseOffset.y, 0, max);
                    scrollView.ScrollToVerticalPercent(offset / max);
                }

                if ((orientation & ScrollbarOrientation.Horizontal) != 0) {
                    float max = scrollView.GetMaxWidth();
                    float offset = Mathf.Clamp(MousePosition.x - scrollView.horizontalTrack.layoutResult.screenPosition.x - baseOffset.x, 0, max);
                    scrollView.ScrollToHorizontalPercent(offset / max);
                }
            }
        }
    }
}