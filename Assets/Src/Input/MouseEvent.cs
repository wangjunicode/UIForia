using UnityEngine;

namespace Src.Input {

    public class MouseInputEvent : InputEvent {

        public readonly Vector2 mousePosition;

        public MouseInputEvent(InputEventType type, Vector2 mousePosition) : base(type) {
            this.mousePosition = mousePosition;
        }

    }

}