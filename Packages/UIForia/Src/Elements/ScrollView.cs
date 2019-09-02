using UIForia.Attributes;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEngine;

namespace UIForia.Elements {

    [Template(TemplateType.Internal, "Elements/ScrollView.xml")]
    public class ScrollView : UIElement {

        public UIElement targetElement { get; protected set; }
        public UIElement verticalHandle { get; protected set; }
        public UIElement verticalTrack { get; protected set; }

        public UIElement horizontalHandle { get; protected set; }
        public UIElement horizontalTrack { get; protected set; }

        public float scrollSpeed = 0.05f;
        public float fadeTime = 2f;

        public bool disableOverflowX;
        public bool disableOverflowY;

        protected float lastScrollVerticalTimestamp;
        protected float lastScrollHorizontalTimestamp;

        protected UIElement childrenElement;
        // todo -- without layout system integration this is an overlay scroll bar only

        public override void OnEnable() {
            targetElement = this; //FindFirstByType<UIChildrenElement>().GetChild(0);
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
                float trackRectHeight = verticalTrack.layoutResult.allocatedSize.height;
                float handleHeight = verticalHandle.layoutResult.allocatedSize.height;
                float max = trackRectHeight - handleHeight;
                float offset = Mathf.Clamp(targetElement.scrollOffset.y - (scrollSpeed * evt.ScrollDelta.y), 0, 1);
                // targetElement.scrollOffset = new Vector2(targetElement.scrollOffset.x, offset);
                // verticalHandle.style.SetTransformPositionY(offset * (max), StyleState.Normal);
                evt.StopPropagation();
            }

            if (horizontalTrack.isEnabled) {
                
                lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
                float trackRectWidth = horizontalTrack.layoutResult.allocatedSize.width;
                float handleWidth = horizontalHandle.layoutResult.allocatedSize.width;
                float max = trackRectWidth - handleWidth;
                float offset = Mathf.Clamp(targetElement.scrollOffset.x - (scrollSpeed * evt.ScrollDelta.x), 0, 1);
                // targetElement.scrollOffset = new Vector2(offset, targetElement.scrollOffset.y);
                // horizontalHandle.style.SetTransformPositionX(offset * (max), StyleState.Normal);
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
            Size allocatedSize = targetElement.layoutResult.allocatedSize;
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

            if (((flags & UIElementFlags.EnabledThisFrame) != 0)) {
              //  return;
            }

            if (disableOverflowY) {
                verticalTrack.SetEnabled(false);
            }

            if (disableOverflowX) {
                horizontalTrack.SetEnabled(false);
            }

            if (overflowSize.width <= allocatedSize.width) {
                horizontalTrack.SetEnabled(false);
                targetElement.scrollOffset = new Vector2(0, targetElement.scrollOffset.y);
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
                targetElement.scrollOffset = new Vector2(targetElement.scrollOffset.x, 0);
            }
            else if (!disableOverflowY) {
                verticalTrack.SetEnabled(true);
                float height = (allocatedSize.height / overflowSize.height) * allocatedSize.height;
                float opacity = 1; //1 - Mathf.Clamp01(Easing.Interpolate((Time.realtimeSinceStartup - lastScrollVerticalTimestamp) / fadeTime, EasingFunction.CubicEaseInOut));
                // verticalHandle.style.SetPreferredHeight(height, StyleState.Normal);
                // verticalTrack.style.SetOpacity(opacity, StyleState.Normal);
                // verticalTrack.style.SetTransformPositionX(layoutResult.allocatedSize.width - verticalTrack.layoutResult.actualSize.width, StyleState.Normal);
            }
        }

        public void OnClickVertical(MouseInputEvent evt) {
            ScrollPageTowardsY(evt.MousePosition.y);
            evt.StopPropagation();
        }

        private void ScrollPageTowardsY(float y) {
            
            if (!verticalTrack.isEnabled) return;
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
            float trackRectHeight = verticalTrack.layoutResult.allocatedSize.height;
            float targetHeight = targetElement.layoutResult.actualSize.height;
            float handleTop = verticalHandle.layoutResult.screenPosition.y;
            float handleBottom = handleTop + verticalHandle.layoutResult.allocatedSize.height;
            float pageSize = trackRectHeight;
            float direction = 0;
            if (y < handleTop) {
                direction = -1;
            }
            else if (y > handleBottom) {
                direction = 1;
            }

            float handleHeight = verticalHandle.layoutResult.allocatedSize.height;
            float max = trackRectHeight - handleHeight;
            float offset = Mathf.Clamp(targetElement.scrollOffset.y + (direction * (pageSize / targetHeight)), 0, 1);
            targetElement.scrollOffset = new Vector2(targetElement.scrollOffset.x, offset);
            verticalHandle.style.SetTransformPositionY(offset * (max), StyleState.Normal);
        }

        public void OnClickHorizontal(MouseInputEvent evt) {
            ScrollPageTowardsX(evt.MousePosition.x);
            evt.StopPropagation();
        }

        private void ScrollPageTowardsX(float x) {
            if (!horizontalTrack.isEnabled) return;
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
            float trackRectWidth = horizontalTrack.layoutResult.allocatedSize.width;
            float targetWidth = targetElement.layoutResult.actualSize.width;
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

            float handleWidth = horizontalHandle.layoutResult.allocatedSize.width;
            float max = trackRectWidth - handleWidth;
            float offset = Mathf.Clamp(targetElement.scrollOffset.x + (direction * (pageSize / targetWidth)), 0, 1);
            targetElement.scrollOffset = new Vector2(offset, targetElement.scrollOffset.y);
            horizontalHandle.style.SetTransformPositionX(offset * (max), StyleState.Normal);
        }

        [OnDragCreate]
        public DragEvent OnMiddleMouseDrag(MouseInputEvent evt) {
            if (!evt.IsMouseMiddleDown) {
                return null;
            }

            Size allocatedSize = targetElement.layoutResult.allocatedSize;
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
            float baseOffset = evt.MousePosition.y - handlePosition;
            return new ScrollbarDragEvent(ScrollbarOrientation.Vertical, new Vector2(0, baseOffset), this);
        }

        protected virtual DragEvent OnCreateHorizontalDrag(MouseInputEvent evt) {
            if (evt.IsMouseRightDown) return null;
            lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
            float handlePosition = horizontalHandle.layoutResult.screenPosition.x;
            float baseOffset = evt.MousePosition.x - handlePosition;
            return new ScrollbarDragEvent(ScrollbarOrientation.Horizontal, new Vector2(baseOffset, 0), this);
        }

        public override void HandleUIEvent(UIEvent evt) {
            if (evt is UIScrollEvent scrollEvent) {
                if (scrollEvent.ScrollDestinationX >= 0) {
                    ScrollToX(scrollEvent.ScrollDestinationX);
                }

                if (scrollEvent.ScrollDestinationY >= 0) {
                    ScrollToY(scrollEvent.ScrollDestinationY);
                }
            }
        }

        private void ScrollToY(float y) {
            if (!verticalTrack.isEnabled) {
                return;
            }
            
            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
            float trackRectHeight = verticalTrack.layoutResult.allocatedSize.height;
            float handleHeight = verticalHandle.layoutResult.allocatedSize.height;
            float max = trackRectHeight - handleHeight;
            float offset = Mathf.Clamp(y, 0, 1);
            targetElement.scrollOffset = new Vector2(targetElement.scrollOffset.x, offset);
            verticalHandle.style.SetTransformPositionY(offset * (max), StyleState.Normal);
        }

        private void ScrollToX(float x) {
            if (!horizontalTrack.isEnabled) {
                return;
            }

            lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
            float trackRectWidth = horizontalTrack.layoutResult.allocatedSize.width;
            float handleWidth = horizontalHandle.layoutResult.allocatedSize.width;
            float max = trackRectWidth - handleWidth;
            float offset = Mathf.Clamp(x, 0, 1);
            targetElement.scrollOffset = new Vector2(offset, targetElement.scrollOffset.y);
            horizontalHandle.style.SetTransformPositionX(offset * (max), StyleState.Normal);
        }

        public class ScrollbarDragEvent : DragEvent {

            public readonly Vector2 baseOffset;
            public readonly ScrollView scrollbar;
            public readonly ScrollbarOrientation orientation;

            public ScrollbarDragEvent(ScrollbarOrientation orientation, Vector2 baseOffset, ScrollView scrollbar) : base(scrollbar.targetElement) {
                this.orientation = orientation;
                this.baseOffset = baseOffset;
                this.scrollbar = scrollbar;
            }

            public override void Update() {
                if ((orientation & ScrollbarOrientation.Vertical) != 0) {
                    scrollbar.lastScrollVerticalTimestamp = Time.realtimeSinceStartup;
                    float trackRectY = scrollbar.verticalTrack.layoutResult.screenPosition.y;
                    float trackRectHeight = scrollbar.verticalTrack.layoutResult.allocatedSize.height;
                    float handleHeight = scrollbar.verticalHandle.layoutResult.allocatedSize.height;
                    float max = trackRectHeight - handleHeight;
                    float offset = Mathf.Clamp(MousePosition.y - trackRectY - baseOffset.y, 0, max);
                    scrollbar.targetElement.scrollOffset = new Vector2(scrollbar.targetElement.scrollOffset.x, offset / max);
                    scrollbar.verticalHandle.style.SetAlignmentOffsetY(new OffsetMeasurement(-1f, OffsetMeasurementUnit.Percent), StyleState.Normal);
                    scrollbar.verticalHandle.style.SetAlignmentOriginY(new OffsetMeasurement(1f, OffsetMeasurementUnit.Percent), StyleState.Normal);
                }

                if ((orientation & ScrollbarOrientation.Horizontal) != 0) {
                    scrollbar.lastScrollHorizontalTimestamp = Time.realtimeSinceStartup;
                    float trackRectX = scrollbar.horizontalTrack.layoutResult.screenPosition.x;
                    float trackRectWidth = scrollbar.horizontalTrack.layoutResult.allocatedSize.width;
                    float handleWidth = scrollbar.horizontalHandle.layoutResult.allocatedSize.width;
                    float max = trackRectWidth - handleWidth;
                    float offset = Mathf.Clamp(MousePosition.x - trackRectX - baseOffset.x, 0, max);
                    scrollbar.targetElement.scrollOffset = new Vector2(offset / max, scrollbar.targetElement.scrollOffset.y);
                    scrollbar.horizontalHandle.style.SetTransformPositionX(offset, StyleState.Normal);
                }
            }

        }

    }

}