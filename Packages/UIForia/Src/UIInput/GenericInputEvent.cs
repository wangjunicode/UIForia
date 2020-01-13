using UnityEngine;

namespace UIForia.UIInput {

    public struct GenericInputEvent {

        public readonly InputEventType type;
        public readonly KeyboardModifiers modifiers;
        public readonly EventPropagator source;
        
        public char character;
        public KeyCode keyCode;
        public bool isFocused;

        public GenericInputEvent(InputEventType eventType, KeyboardModifiers modifiers, EventPropagator source, char character, KeyCode keyCode, bool isFocused) {
            this.type = eventType;
            this.modifiers = modifiers;
            this.source = source;
            this.character = character;
            this.keyCode = keyCode;
            this.isFocused = isFocused;
        }

        public MouseInputEvent AsMouseInputEvent => new MouseInputEvent(source, type, modifiers);
        
        public KeyboardInputEvent AsKeyInputEvent => new KeyboardInputEvent(type, keyCode, character, modifiers, isFocused);

    }

}