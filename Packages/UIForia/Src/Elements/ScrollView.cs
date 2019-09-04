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

        public float scrollSpeed = 6.66f;
        public float fadeTime = 2f;

        public bool disableOverflowX;
        public bool disableOverflowY;

        protected float lastScrollVerticalTimestamp;
        protected float lastScrollHorizontalTimestamp;

        protected UIElement childrenElement;
        // todo -- without layout system integration this is an overlay scroll bar only

        public override void OnEnable() {
            childrenElement = FindById("scroll-root");
            verticalHandle = FindById("scroll-handle-vertical");
            verticalTrack = FindById("scroll-track-vertical");
            horizontalHandle = FindById("scroll-handle-horizontal");
            horizontalTrack = FindById("scroll-track-horizontal");
        }
        
        [OnMouseWheel]
        public void OnMouseWheel(MouseInputEvent evt) {

            if (verticalTrack.isEnabled) {
                lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
                float trackRectHeight = verticalTrack.layoutResult.actualSize.height;
                float handleHeight = verticalHandle.layoutResult.actualSize.height;
                float max = trackRectHeight - handleHeight;
                float offset = (verticalHandle.layoutResult.screenPosition.y - verticalTrack.layoutResult.screenPosition.y) + (scrollSpeed * -evt.ScrollDelta.y);
                offset = Mathf.Clamp(offset, 0, max);
                ScrollToVerticalPercent(offset / max);
                evt.StopPropagation();
            }

            if (horizontalTrack.isEnabled) {
                
                lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
                float trackRectWidth = horizontalTrack.layoutResult.actualSize.width;
                float handleWidth = horizontalHandle.layoutResult.actualSize.width;
                float max = trackRectWidth - handleWidth;
                float offset = (horizontalHandle.layoutResult.screenPosition.x - horizontalTrack.layoutResult.screenPosition.x) + (scrollSpeed * -evt.ScrollDelta.x);
                float scrollPixels = overflowSize.width - layoutResult.actualSize.width;
                offset = Mathf.Clamp(offset, 0, max);
                float percentage = offset / max;
                horizontalHandle.style.SetAlignmentOffsetX(new OffsetMeasurement(-percentage, OffsetMeasurementUnit.Percent), StyleState.Normal);
                horizontalHandle.style.SetAlignmentOriginX(new OffsetMeasurement(percentage, OffsetMeasurementUnit.Percent), StyleState.Normal);
                childrenElement.style.SetTransformPositionX(new OffsetMeasurement(-percentage * scrollPixels), StyleState.Normal);
                evt.StopPropagation();
            }
        }

        public void OnHoverHorizontal(MouseInputEvent evt) {
            lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
        }

        public void OnHoverVertical(MouseInputEvent evt) {
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
        }

        private Size overflowSize;
        
        public override void OnUpdate() {
            Size allocatedSize = layoutResult.allocatedSize;
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            
            for (int i = 0; i < childrenElement.children.size; i++) {
                if (childrenElement.children[i].style.ClipBehavior == ClipBehavior.Normal) {
                    Rect screenRect = childrenElement.children[i].layoutResult.ScreenRect;
                    if (screenRect.x < minX) minX = screenRect.x;
                    if (screenRect.y < minY) minY = screenRect.y;
                    if (screenRect.xMax > maxX) maxX = screenRect.xMax;
                    if (screenRect.yMax > maxY) maxY = screenRect.yMax;
                }
            }

            overflowSize = new Size(maxX - minX, maxY - minY);

            if (disableOverflowY) {
               verticalTrack.SetEnabled(false);
            }

            if (disableOverflowX) {
                horizontalTrack.SetEnabled(false);
            }

            if (overflowSize.width <= allocatedSize.width) {
                horizontalTrack.SetEnabled(false);
            }
            else if (!disableOverflowX) {
                horizontalTrack.SetEnabled(true);
                float width = (allocatedSize.width / overflowSize.width) * allocatedSize.width;
                float opacity = 1 - Mathf.Clamp01(Easing.Interpolate((Time.realtimeSinceStartup - lastScrollHorizontalTimestamp) / fadeTime, EasingFunction.CubicEaseInOut));
                // horizontalHandle.style.SetPreferredWidth(width, StyleState.Normal);
                // horizontalTrack.style.SetOpacity(opacity, StyleState.Normal);
                // horizontalTrack.style.SetTransformPositionY(layoutResult.allocatedSize.height - horizontalTrack.layoutResult.actualSize.height, StyleState.Normal);
            }

            if (overflowSize.height <= allocatedSize.height) {
                verticalTrack.SetEnabled(false);
            }
            else if (!disableOverflowY) {
                verticalTrack.SetEnabled(true);
                float height = (allocatedSize.height / overflowSize.height) * allocatedSize.height;
                float opacity = 1 - Mathf.Clamp01(Easing.Interpolate((Time.realtimeSinceStartup - lastScrollVerticalTimestamp) / fadeTime, EasingFunction.CubicEaseInOut));
                verticalHandle.style.SetPreferredHeight(height, StyleState.Normal);
                verticalTrack.style.SetOpacity(opacity, StyleState.Normal);
            }
        }

        public void OnClickVertical(MouseInputEvent evt) {
            ScrollPageTowardsY(evt.MousePosition.y);
            evt.StopPropagation();
        }

        private void ScrollPageTowardsY(float y) {
            
            if (!verticalTrack.isEnabled) return;
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
            float trackRectHeight = verticalTrack.layoutResult.actualSize.height;
            float handleTop = verticalHandle.layoutResult.screenPosition.y;
            float handleBottom = handleTop + verticalHandle.layoutResult.actualSize.height;
            float direction = 0;
            
            if (y < handleTop) {
                direction = -1;
            }
            else if (y > handleBottom) {
                direction = 1;
            }

            float handleHeight = verticalHandle.layoutResult.actualSize.height;
            float max = trackRectHeight - handleHeight;
            float offset = (verticalHandle.layoutResult.screenPosition.y - verticalTrack.layoutResult.screenPosition.y);
            offset += (trackRectHeight / overflowSize.height ) * direction;
            ScrollToVerticalPercent(offset / max);
        }

        public void OnClickHorizontal(MouseInputEvent evt) {
            ScrollPageTowardsX(evt.MousePosition.x);
            evt.StopPropagation();
        }

        private void ScrollPageTowardsX(float x) {
            if (!horizontalTrack.isEnabled) return;
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
            float trackRectWidth = horizontalTrack.layoutResult.allocatedSize.width;
            float targetWidth = layoutResult.actualSize.width;
            float handleLeft = horizontalHandle.layoutResult.screenPosition.x;
            float handleRight = handleLeft + horizontalHandle.layoutResult.allocatedSize.width;
            float pageSize = trackRectWidth;
            float direction = 0;
            if (x < handleLeft) {
                direction = -1;
            }
            else if (x > handleRight) {
                direction = 1;
            }

            float offset = 0; // todo -- fix
            
            float handleWidth = horizontalHandle.layoutResult.allocatedSize.width;
            float max = trackRectWidth - handleWidth;
            horizontalHandle.style.SetTransformPositionX(offset * (max), StyleState.Normal);
        }

        [OnDragCreate]
        public DragEvent OnMiddleMouseDrag(MouseInputEvent evt) {
            if (!evt.IsMouseMiddleDown) {
                return null;
            }

            Size allocatedSize = layoutResult.allocatedSize;
            Vector2 baseOffset = new Vector2();
            ScrollbarOrientation orientation = 0;

            if (!disableOverflowX && overflowSize.width > allocatedSize.width) {
                lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
                baseOffset.x = evt.MousePosition.x - horizontalHandle.layoutResult.screenPosition.x;
                orientation |= ScrollbarOrientation.Horizontal;
            }

            if (!disableOverflowY && overflowSize.height > allocatedSize.height) {
                lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
                baseOffset.y = evt.MousePosition.y - verticalHandle.layoutResult.screenPosition.y;
                orientation |= ScrollbarOrientation.Vertical;
            }

            return new ScrollbarDragEvent(orientation, baseOffset, this);
        }

        protected virtual DragEvent OnCreateVerticalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
            float handlePosition = verticalHandle.layoutResult.screenPosition.y;
            float baseOffset = evt.MouseDownPosition.y - handlePosition;
            return new ScrollbarDragEvent(ScrollbarOrientation.Vertical, new Vector2(0, baseOffset), this);
        }

        protected virtual DragEvent OnCreateHorizontalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
            float handlePosition = horizontalHandle.layoutResult.screenPosition.x;
            float baseOffset = evt.MouseDownPosition.x - handlePosition;
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

        private void ScrollToVerticalPercent(float percentage) {
            percentage = Mathf.Clamp01(percentage);
            float scrollPixels = overflowSize.height - layoutResult.actualSize.height;
            verticalHandle.style.SetAlignmentOffsetY(new OffsetMeasurement(-percentage, OffsetMeasurementUnit.Percent), StyleState.Normal);
            verticalHandle.style.SetAlignmentOriginY(new OffsetMeasurement(percentage, OffsetMeasurementUnit.Percent), StyleState.Normal);
            childrenElement.style.SetTransformPositionY(new OffsetMeasurement(-percentage * scrollPixels), StyleState.Normal);
        }

        private void ScrollToHorizontalPercent(float percentage) {
            if (!horizontalTrack.isEnabled) {
                return;
            }
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
            percentage = Mathf.Clamp01(percentage);
            float scrollPixels = overflowSize.width - layoutResult.actualSize.width;
            horizontalHandle.style.SetAlignmentOffsetX(new OffsetMeasurement(-percentage, OffsetMeasurementUnit.Percent), StyleState.Normal);
            horizontalHandle.style.SetAlignmentOriginX(new OffsetMeasurement(percentage, OffsetMeasurementUnit.Percent), StyleState.Normal);
            childrenElement.style.SetTransformPositionX(new OffsetMeasurement(-percentage * scrollPixels), StyleState.Normal);
        }

        public class ScrollbarDragEvent : DragEvent {

            public readonly Vector2 baseOffset;
            public readonly ScrollView scrollbar;
            public readonly ScrollbarOrientation orientation;

            public ScrollbarDragEvent(ScrollbarOrientation orientation, Vector2 baseOffset, ScrollView scrollbar) : base(scrollbar) {
                this.orientation = orientation;
                this.baseOffset = baseOffset;
                this.scrollbar = scrollbar;
            }

            public override void Update() {
                if ((orientation & ScrollbarOrientation.Vertical) != 0) {
                    scrollbar.lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
                    float trackRectY = scrollbar.verticalTrack.layoutResult.screenPosition.y;
                    float trackRectHeight = scrollbar.verticalTrack.layoutResult.actualSize.height;
                    float handleHeight = scrollbar.verticalHandle.layoutResult.actualSize.height;
                    float max = trackRectHeight - handleHeight;
                    float offset = Mathf.Clamp(MousePosition.y - trackRectY - baseOffset.y, 0, max);
                    float percentage = offset / max;
                    scrollbar.verticalHandle.style.SetAlignmentOffsetY(new OffsetMeasurement(-percentage, OffsetMeasurementUnit.Percent), StyleState.Normal);
                    scrollbar.verticalHandle.style.SetAlignmentOriginY(new OffsetMeasurement(percentage, OffsetMeasurementUnit.Percent), StyleState.Normal);
                    float scrollPixels = scrollbar.overflowSize.height - scrollbar.layoutResult.actualSize.height;
                    scrollbar.childrenElement.style.SetTransformPositionY(new OffsetMeasurement(-percentage * scrollPixels), StyleState.Normal);
                }

                if ((orientation & ScrollbarOrientation.Horizontal) != 0) {
                    scrollbar.lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
                    float trackRectX = scrollbar.horizontalTrack.layoutResult.screenPosition.x;
                    float trackRectWidth = scrollbar.horizontalTrack.layoutResult.actualSize.width;
                    float handleWidth = scrollbar.horizontalHandle.layoutResult.actualSize.width;
                    float max = trackRectWidth - handleWidth;
                    float offset = Mathf.Clamp(MousePosition.x - trackRectX - baseOffset.x, 0, max);
                    float percentage = offset / max;
                    scrollbar.horizontalHandle.style.SetAlignmentOffsetX(new OffsetMeasurement(-percentage, OffsetMeasurementUnit.Percent), StyleState.Normal);
                    scrollbar.horizontalHandle.style.SetAlignmentOriginX(new OffsetMeasurement(percentage, OffsetMeasurementUnit.Percent), StyleState.Normal);
                }
            }

        }

    }

}