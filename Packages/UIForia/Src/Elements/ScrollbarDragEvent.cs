using UIForia.Input;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Elements {

    public class ScrollbarDragEvent : DragEvent {

        public readonly float baseOffset;
        public readonly VirtualScrollbar scrollbar;

        public ScrollbarDragEvent(float baseOffset, VirtualScrollbar scrollbar) : base(scrollbar.targetElement) {
            this.baseOffset = baseOffset;
            this.scrollbar = scrollbar;
        }

        public override void Update() {
            Rect trackRect = scrollbar.trackRect;
            if (scrollbar.orientation == ScrollbarOrientation.Vertical) {
                float max = trackRect.height - scrollbar.handleHeight;
                float offset = Mathf.Clamp(MousePosition.y - trackRect.y - baseOffset, 0, max);
                scrollbar.targetElement.scrollOffset = new Vector2(scrollbar.targetElement.scrollOffset.x, offset / max);
                scrollbar.handleRect = new Rect(scrollbar.handleRect) {
                    y = offset
                };
            }
            else {
                float max = trackRect.width - scrollbar.handleWidth;
                float offset = Mathf.Clamp(MousePosition.x - trackRect.x - baseOffset, 0, max);
                scrollbar.targetElement.scrollOffset = new Vector2(offset / max, scrollbar.targetElement.scrollOffset.y);
                scrollbar.handleRect = new Rect(scrollbar.handleRect) {
                    x = offset
                };
            }
        }

    }

}