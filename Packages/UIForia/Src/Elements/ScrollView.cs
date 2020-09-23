using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.UIInput;
using UIForia.Util.Unsafe;
using Unity.Collections;
using UnityEngine;

namespace UIForia.Elements {

    public enum ScrollGutterSide {

        Max,
        Min

    }

    // shared unmanaged struct so the layout system can access and modify these values
    internal struct ScrollValues {

        public float scrollX;
        public float scrollY;
        public float contentWidth;
        public float contentHeight;
        public bool isOverflowingX => contentWidth > actualWidth;
        public bool isOverflowingY => contentHeight > actualHeight;
        public float actualWidth;
        public float actualHeight;
        public ElementId verticalTrackId;
        public ElementId horizontalTrackId;
        public float trackSize;
        public ScrollGutterSide horizontalGutterPosition;
        public ScrollGutterSide verticalGutterPosition;
        public bool showHorizontal;
        public bool showVertical;

    }

    [Template(TemplateType.Internal, "Elements/ScrollView.xml#horizontal-scroll-track")]
    public class HorizontalScrollTrack : UIElement {

        protected ScrollView scrollView;
        protected UIElement handle;

        public float handleWidth;

        public override void OnCreate() {
            scrollView = FindParent<ScrollView>();
            handle = FindById("handle");
        }

        public override void OnUpdate() {
            handleWidth = scrollView.GetScrollPagePercentageX() * layoutResult.ActualWidth;
            handle.style.SetPreferredWidth(new UIMeasurement(handleWidth), StyleState.Normal);
            handle.style.SetAlignmentOriginX(OffsetMeasurement.Percent(scrollView.scrollPercentageX), StyleState.Normal);
            handle.style.SetAlignmentOffsetX(OffsetMeasurement.Percent(-scrollView.scrollPercentageX), StyleState.Normal);
        }

        [OnDragCreate]
        public virtual DragEvent OnCreateHandleDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            float baseOffset = evt.MousePosition.x - (evt.element.layoutResult.screenPosition.x);
            return new ScrollHorizontalHandleDragEvent(scrollView, this, handle, baseOffset);
        }

        [OnMouseClick]
        public void OnClick(MouseInputEvent evt) {

            if (evt.Origin != this) {
                evt.StopPropagation();
                return;
            }

            float percentage = scrollView.scrollPercentageX;
            float relative = evt.MousePosition.x - layoutResult.screenPosition.x;
            float relativePercentage = relative / layoutResult.ActualWidth;

            if (relativePercentage > percentage) {
                scrollView.ScrollToHorizontalPercent(percentage + scrollView.GetScrollPagePercentageX());
            }
            else {
                scrollView.ScrollToHorizontalPercent(percentage - scrollView.GetScrollPagePercentageX());
            }

            evt.StopPropagation();
        }

        public class ScrollHorizontalHandleDragEvent : DragEvent {

            protected ScrollView scrollView;
            protected UIElement handle;
            protected UIElement track;
            private float offset;
            private float scrollPercentageOffset;

            public ScrollHorizontalHandleDragEvent(ScrollView scrollView, UIElement track, UIElement handle, float offset) {
                this.scrollView = scrollView;
                this.handle = handle;
                this.track = track;
                this.offset = offset;
                this.scrollPercentageOffset = scrollView.scrollPercentageX;
            }

            public override void Update() {
                float width = scrollView.GetHorizontalGutterWidth() - handle.layoutResult.ActualWidth;

                float x = MousePosition.x - (track.layoutResult.screenPosition.x) - offset;

                if (width == 0) {
                    scrollView.ScrollToHorizontalPercent(0);
                }
                else {
                    float percentage = (x / width) + scrollPercentageOffset;
                    scrollView.ScrollToHorizontalPercent(percentage);
                }

            }

        }

    }

    [Template(TemplateType.Internal, "Elements/ScrollView.xml#vertical-scroll-track")]
    public class VerticalScrollTrack : UIElement {

        protected ScrollView scrollView;
        protected UIElement handle;

        public float handleHeight;

        public override void OnCreate() {
            scrollView = FindParent<ScrollView>();
            handle = FindById("handle");
        }

        public override void OnUpdate() {
            handleHeight = scrollView.GetScrollPagePercentageY() * layoutResult.ActualHeight;
            handle.style.SetPreferredHeight(new UIMeasurement(handleHeight), StyleState.Normal);
            handle.style.SetAlignmentOriginY(OffsetMeasurement.Percent(scrollView.scrollPercentageY), StyleState.Normal);
            handle.style.SetAlignmentOffsetY(OffsetMeasurement.Percent(-scrollView.scrollPercentageY), StyleState.Normal);
        }

        [OnDragCreate]
        public virtual DragEvent OnCreateHandleDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            float baseOffset = evt.MousePosition.y - (evt.element.layoutResult.screenPosition.y);
            return new ScrollVerticalHandleDragEvent(scrollView, this, handle, baseOffset);
        }

        [OnMouseClick]
        public void OnClick(MouseInputEvent evt) {

            if (evt.Origin != this) {
                evt.StopPropagation();
                return;
            }

            float percentage = scrollView.scrollPercentageY;
            float relative = evt.MousePosition.y - layoutResult.screenPosition.y;
            float relativePercentage = relative / layoutResult.ActualHeight;

            if (relativePercentage > percentage) {
                scrollView.ScrollToVerticalPercent(percentage + scrollView.GetScrollPagePercentageY());
            }
            else {
                scrollView.ScrollToVerticalPercent(percentage - scrollView.GetScrollPagePercentageY());
            }

            evt.StopPropagation();
        }

        public class ScrollVerticalHandleDragEvent : DragEvent {

            protected ScrollView scrollView;
            protected UIElement handle;
            protected UIElement track;
            private float offset;
            private float scrollPercentageOffset;

            public ScrollVerticalHandleDragEvent(ScrollView scrollView, UIElement track, UIElement handle, float offset) {
                this.scrollView = scrollView;
                this.handle = handle;
                this.track = track;
                this.offset = offset;
                this.scrollPercentageOffset = scrollView.scrollPercentageY;
            }

            public override void Update() {
                float height = scrollView.GetVerticalGutterHeight() - handle.layoutResult.ActualHeight;

                float y = MousePosition.y - (track.layoutResult.screenPosition.y) - offset;

                if (height == 0) {
                    scrollView.ScrollToVerticalPercent(0);
                }
                else {
                    float percentage = (y / height) + scrollPercentageOffset;
                    scrollView.ScrollToVerticalPercent(percentage);
                }

            }

        }

    }

    [Template(TemplateType.Internal, "Elements/ScrollView.xml")]
    public unsafe class ScrollView : UIElement {

        public float scrollSpeedY = 48f;
        public float scrollSpeedX = 16f;

        protected UIElement verticalScrollTrack;
        protected UIElement horizontalScrollTrack;

        public float trackSize {
            get {
                InitScrollValues();
                return scrollValues->trackSize;
            }
            set {
                InitScrollValues();
                scrollValues->trackSize = Mathf.Min(1, value);
            }
        }

        public bool disableOverflowX;
        public bool disableOverflowY;

        public bool disableAutoScroll = false;

        public bool verticalScrollingEnabled => !disableOverflowY && isOverflowingY;
        public bool horizontalScrollingEnabled => !disableOverflowX && isOverflowingX;

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

        private void InitScrollValues() {
            if (scrollValues != null) {
                return;
            }

            verticalScrollTrack = FindById("vertical-scrolltrack");
            horizontalScrollTrack = FindById("horizontal-scrolltrack");

            scrollValues = TypedUnsafe.Malloc<ScrollValues>(Allocator.Persistent);
            *scrollValues = default;
            scrollValues->trackSize = 10;
            scrollValues->horizontalGutterPosition = ScrollGutterSide.Max;
            scrollValues->verticalGutterPosition = ScrollGutterSide.Max;
            scrollValues->verticalTrackId = verticalScrollTrack.id;
            scrollValues->horizontalTrackId = horizontalScrollTrack.id;
        }

        internal float scrollPercentageX {
            get {
                InitScrollValues();
                return scrollValues->scrollX;
            }
            set {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                InitScrollValues();
                scrollValues->scrollX = value;
            }
        }

        internal float scrollPercentageY {
            get {
                InitScrollValues();
                return scrollValues->scrollY;
            }
            set {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                InitScrollValues();
                scrollValues->scrollY = value;
            }
        }

        internal ScrollValues* GetScrollValues() {
            InitScrollValues();
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

                float t = Mathf.Clamp01(Easing.Interpolate(elapsedTotalTime / 0.5f, EasingFunction.CubicEaseOut));
                scrollPercentageX = Mathf.Lerp(fromScrollX, toScrollX, t);
                isScrollingX = t < 1;
            }

            InitScrollValues();
            verticalScrollTrack.SetEnabled(verticalScrollingEnabled);
            horizontalScrollTrack.SetEnabled(horizontalScrollingEnabled);

            scrollValues->showVertical = verticalScrollTrack.isEnabled;
            scrollValues->showHorizontal = horizontalScrollTrack.isEnabled;

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

        public float GetScrollPagePercentageX() {
            InitScrollValues();
            if (scrollValues->contentWidth <= 0) return 0;
            float pageSize = scrollValues->actualWidth / scrollValues->contentWidth;
            return Mathf.Clamp01(pageSize);
        }

        public float GetScrollPagePercentageY() {
            InitScrollValues();
            if (scrollValues->contentHeight <= 0) return 0;
            float pageSize = scrollValues->actualHeight / scrollValues->contentHeight;
            return Mathf.Clamp01(pageSize);
        }

        public float GetVerticalGutterHeight() {
            InitScrollValues();
            return scrollValues->actualHeight;
        }

        public float GetHorizontalGutterWidth() {
            InitScrollValues();
            return scrollValues->actualWidth;
        }

    }

}