using UnityEngine;

namespace Src.Input {

    // can't be a struct because propagation is not read only
    public class KeyboardInputEvent : InputEvent {

        public readonly KeyCode keyCode;
        public readonly EventModifiers modifiers;

        public readonly bool isFocused;
        
        public KeyboardInputEvent(InputEventType type, KeyCode keyCode, EventModifiers modifiers, bool isFocused) : base(type) {
            this.keyCode = keyCode;
            this.modifiers = modifiers;
            this.isFocused = isFocused;
        }

        public void StopPropagation() {
            stopPropagation = true;
        }

        public void StopPropagationImmediately() {
            stopPropagationImmediately = true;
        }
        
        public bool stopPropagation { get; private set; }
        public bool stopPropagationImmediately { get; private set; }
        
        public bool alt => (modifiers & EventModifiers.Alt) != 0;

        public bool shift => (modifiers & EventModifiers.Shift) != 0;

        public bool ctrl => (modifiers & EventModifiers.Control) != 0;

        public bool onlyControl => ctrl && !alt && !shift;

        public bool fn => (modifiers & EventModifiers.FunctionKey) != 0;

        public bool command => (modifiers & EventModifiers.Command) != 0;

        public bool numLock => (modifiers & EventModifiers.Numeric) != 0;

        public bool capsLock => (modifiers & EventModifiers.CapsLock) != 0;

    }

}