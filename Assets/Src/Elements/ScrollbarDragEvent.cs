using Src.Rendering;
using Src.Input;
using UnityEngine;

namespace Src.Elements {

    public class ScrollbarDragEvent : DragEvent {

        public readonly float baseOffset;
        public readonly VirtualScrollbar scrollbar;

        public ScrollbarDragEvent(float baseOffset, VirtualScrollbar scrollbar) {
            this.baseOffset = baseOffset;
            this.scrollbar = scrollbar;
        }

        public override void Update() {
            Rect trackRect = scrollbar.TrackRect;
            if (scrollbar.orientation == ScrollbarOrientation.Vertical) {
                float max = trackRect.height - scrollbar.handleHeight;
                float offset = Mathf.Clamp(MousePosition.y - trackRect.y - baseOffset, 0, max);
                scrollbar.targetElement.scrollOffset = new Vector2(scrollbar.targetElement.scrollOffset.x, offset / max);
                scrollbar.handle.style.SetTransformPosition(new Vector2(0f, offset), StyleState.Normal);
            }
            else {
                float max = trackRect.width - scrollbar.handleWidth;
                float offset = Mathf.Clamp(MousePosition.x - trackRect.x - baseOffset, 0, max);
                scrollbar.targetElement.scrollOffset = new Vector2(offset / max, scrollbar.targetElement.scrollOffset.y);
                scrollbar.handle.style.SetTransformPosition(new Vector2(offset, 0f), StyleState.Normal);
            }
        }

    }

}