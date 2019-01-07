using UIForia.Input;
using UIForia.Rendering;
using UnityEngine;

namespace UI {

    public class WindowDragEvent : DragEvent {

        private readonly Vector2 offset;

        public WindowDragEvent(UIElement origin, Vector2 offset) : base(origin) {
            this.offset = offset;
        }

        public override void Update() {
            origin.style.SetTransformPosition(MousePosition - offset, StyleState.Normal);
        }

    }

}