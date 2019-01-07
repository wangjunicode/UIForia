using UIForia.Input;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEngine;

namespace UI {

    public class WindowResizeEvent : DragEvent {

        private readonly WindowSide windowSide;
        private readonly Size originalSize;
        private readonly Vector2 originalLocalPosition;
        private readonly Vector2 offset;
        
        public WindowResizeEvent(UIElement origin, WindowSide side, Vector2 offset) : base(origin) {
            this.windowSide = side;
            this.offset = offset;
            this.originalLocalPosition = origin.layoutResult.localPosition;
            this.originalSize = origin.layoutResult.actualSize;
        }

        public override void Update() {
            Vector2 localMouse = MousePosition - origin.layoutResult.screenPosition;

            if ((windowSide & WindowSide.Top) != 0) {
                float y = origin.layoutResult.localPosition.y + localMouse.y;
                float height = originalSize.height + (originalLocalPosition.y - y);
                origin.style.SetTransformPositionY(y, StyleState.Normal);
                origin.style.SetPreferredHeight(height, StyleState.Normal);
            }

            if ((windowSide & WindowSide.Bottom) != 0) {
                origin.style.SetPreferredHeight(localMouse.y, StyleState.Normal);
            }

            if ((windowSide & WindowSide.Left) != 0) {
                float x = origin.layoutResult.localPosition.x + localMouse.x;
                float width = originalSize.width + (originalLocalPosition.x - x);
                origin.style.SetTransformPositionX(x, StyleState.Normal);
                origin.style.SetPreferredWidth(width, StyleState.Normal);
            }

            if ((windowSide & WindowSide.Right) != 0) {
                origin.style.SetPreferredWidth(localMouse.x, StyleState.Normal);
            }
        }

    }

}