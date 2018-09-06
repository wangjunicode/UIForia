using UnityEngine;

namespace Src.Input {

    public class KeyboardInputEvent : InputEvent {

        public readonly bool isFocused;
        public readonly KeyCode keyCode;
        public readonly KeyboardModifiers modifiers;

        public KeyboardInputEvent(InputEventType type, KeyCode keyCode, KeyboardModifiers modifiers, bool isFocused) : base(type) {
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

        internal bool stopPropagation { get; private set; }
        internal bool stopPropagationImmediately { get; private set; }

        public bool alt => (modifiers & KeyboardModifiers.Alt) != 0;

        public bool shift => (modifiers & KeyboardModifiers.Shift) != 0;

        public bool ctrl => (modifiers & KeyboardModifiers.Control) != 0;

        public bool onlyControl => ctrl && !alt && !shift;

        public bool command => (modifiers & KeyboardModifiers.Command) != 0;

        public bool numLock => (modifiers & KeyboardModifiers.NumLock) != 0;

        public bool capsLock => (modifiers & KeyboardModifiers.CapsLock) != 0;

        public bool windows => (modifiers & KeyboardModifiers.Windows) != 0;

    }

}