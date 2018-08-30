using UnityEngine;

namespace Src.Input {

    public class KeyboardInputEvent : InputEvent {

        public readonly KeyCode keyCode;
        
        public KeyboardInputEvent(InputEventType type, KeyCode keyCode) : base(type) {
            this.keyCode = keyCode;
        }

    }

}