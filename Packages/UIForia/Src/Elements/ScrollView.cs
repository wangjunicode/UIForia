using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.UIInput;
using UnityEngine;

namespace UIForia.Elements {

    [Template(TemplateType.Internal, "Elements/ScrollView.xml")]
    public class ScrollView : UIElement {

        protected UIElement targetElement;
        protected UIElement verticalHandle;
        protected UIElement verticalTrack;
        
        protected UIElement horizontalHandle;
        protected UIElement horizontalTrack;
        
        public override void OnReady() {
            targetElement = FindFirstByType<UIChildrenElement>().GetChild(0);
            verticalHandle = FindById("scroll-handle-vertical");
            verticalTrack = FindById("scroll-track-vertical");
            horizontalHandle = FindById("scroll-handle-horizontal");
            horizontalTrack = FindById("scroll-track-horizontal");
        }

        [OnMouseWheel]
        public void OnMouseWheel(MouseInputEvent evt) {
            float trackRectHeight = verticalTrack.layoutResult.allocatedSize.height;
            float handleHeight = verticalHandle.layoutResult.allocatedSize.height;
            float max = trackRectHeight - handleHeight;
            float offset = Mathf.Clamp(targetElement.scrollOffset.y + (0.05f * evt.ScrollDelta.y), 0, max);
            targetElement.scrollOffset = new Vector2(targetElement.scrollOffset.x, offset / max);
            verticalHandle.style.SetTransformPositionY(offset * trackRectHeight, StyleState.Normal);
            Debug.Log("offset: " + offset + " " + max);
        }
        
        public override void OnUpdate() {
            
//            if (targetElement.layoutResult.IsOverflowingHorizontally) {
//                
//            }
//
//            if (targetElement.layoutResult.IsOverflowingVertically) {
//                
//            }
//            
            float width = (targetElement.layoutResult.allocatedSize.width / targetElement.layoutResult.actualSize.width) * targetElement.layoutResult.allocatedSize.width;
            float height = (targetElement.layoutResult.allocatedSize.height / targetElement.layoutResult.actualSize.height) * targetElement.layoutResult.allocatedSize.height;
            horizontalHandle.style.SetPreferredWidth(width, StyleState.Normal);
            verticalHandle.style.SetPreferredHeight(height, StyleState.Normal);
        }

        protected virtual DragEvent OnCreateVerticalDrag(MouseInputEvent evt) {
            float trackRectY = verticalTrack.layoutResult.screenPosition.y;
            float handlePosition = verticalHandle.layoutResult.screenPosition.y;
            float baseOffset = evt.MouseDownPosition.y - (trackRectY + handlePosition);
            return new ScrollbarDragEvent(ScrollbarOrientation.Vertical, baseOffset, this);
        }
        
        protected virtual DragEvent OnCreateHorizontalDrag(MouseInputEvent evt) {
            float trackRectX = horizontalTrack.layoutResult.screenPosition.x;
            float handlePosition = horizontalHandle.layoutResult.screenPosition.x;
            float baseOffset = evt.MouseDownPosition.x - (trackRectX + handlePosition);
            return new ScrollbarDragEvent(ScrollbarOrientation.Horizontal, baseOffset, this);
        }

        public class ScrollbarDragEvent : DragEvent {

            public readonly float baseOffset;
            public readonly ScrollView scrollbar;
            public readonly ScrollbarOrientation orientation;

            public ScrollbarDragEvent(ScrollbarOrientation orientation, float baseOffset, ScrollView scrollbar) : base(scrollbar.targetElement) {
                this.orientation = orientation;
                this.baseOffset = baseOffset;
                this.scrollbar = scrollbar;
            }

            public override void Update() {
                if (orientation == ScrollbarOrientation.Vertical) {
                    float trackRectY = scrollbar.verticalTrack.layoutResult.screenPosition.y;
                    float trackRectHeight = scrollbar.verticalTrack.layoutResult.allocatedSize.height;
                    float handleHeight = scrollbar.verticalHandle.layoutResult.allocatedSize.height;
                    float max = trackRectHeight - handleHeight;
                    float offset = Mathf.Clamp(MousePosition.y - trackRectY - baseOffset, 0, max);
                    scrollbar.targetElement.scrollOffset = new Vector2(scrollbar.targetElement.scrollOffset.x, offset / max);
                    scrollbar.verticalHandle.style.SetTransformPositionY(offset, StyleState.Normal);
                }
                else {
                    float trackRectX = scrollbar.horizontalTrack.layoutResult.screenPosition.x;
                    float trackRectWidth = scrollbar.horizontalTrack.layoutResult.allocatedSize.width;
                    float handleWidth = scrollbar.horizontalHandle.layoutResult.allocatedSize.width;
                    float max = trackRectWidth - handleWidth;
                    float offset = Mathf.Clamp(MousePosition.x - trackRectX - baseOffset, 0, max);
                    scrollbar.targetElement.scrollOffset = new Vector2(offset / max, scrollbar.targetElement.scrollOffset.y);
                    scrollbar.horizontalHandle.style.SetTransformPositionX(offset, StyleState.Normal);
                }

//                }
//                else {
//                    float max = trackRect.width - scrollbar.handleWidth;
//                    float offset = Mathf.Clamp(MousePosition.x - trackRect.x - baseOffset, 0, max);
//                    scrollbar.targetElement.scrollOffset = new Vector2(offset / max, scrollbar.targetElement.scrollOffset.y);
//                    scrollbar.handleRect = new Rect(scrollbar.handleRect) {
//                        x = offset
//                    };
//                }
            }

        }

    }

}