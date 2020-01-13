using UnityEngine;

namespace UIForia.UIInput {

    public struct GenericInputEvent {

        public readonly InputEventType type;
        public readonly KeyboardModifiers modifiers;
        public readonly EventPropagator propagator;

        public char character;
        public KeyCode keyCode;
        public bool isFocused;

        public GenericInputEvent(InputEventType eventType, KeyboardModifiers modifiers, EventPropagator propagator, char character, KeyCode keyCode, bool isFocused) {
            this.type = eventType;
            this.modifiers = modifiers;
            this.propagator = propagator;
            this.character = character;
            this.keyCode = keyCode;
            this.isFocused = isFocused;
        }

        public MouseInputEvent AsMouseInputEvent => new MouseInputEvent(propagator, type, modifiers);

        public KeyboardInputEvent AsKeyInputEvent => new KeyboardInputEvent(type, keyCode, character, modifiers, isFocused);

    }

}