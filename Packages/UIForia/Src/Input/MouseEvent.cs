using UnityEngine;

namespace UIForia.Input {

    public struct MouseInputEvent {

        public readonly InputEventType type;
        public readonly KeyboardModifiers modifiers;
        
        private readonly EventPropagator source;
        
        public MouseInputEvent(EventPropagator source, InputEventType type, KeyboardModifiers modifiers) {
            this.source = source;
            this.type = type;
            this.modifiers = modifiers;
        }

        public void Consume() {
            source.isConsumed = true;
        }
        
        public void StopPropagation() {
            source.shouldStopPropagation = true;
        }

        public bool IsConsumed => source.isConsumed;
        
        public bool Alt => (modifiers & KeyboardModifiers.Alt) != 0;

        public bool Shift => (modifiers & KeyboardModifiers.Shift) != 0;

        public bool Ctrl => (modifiers & KeyboardModifiers.Control) != 0;

        public bool OnlyControl => Ctrl && !Alt && !Shift;

        public bool Command => (modifiers & KeyboardModifiers.Command) != 0;

        public bool NumLock => (modifiers & KeyboardModifiers.NumLock) != 0;

        public bool CapsLock => (modifiers & KeyboardModifiers.CapsLock) != 0;

        public bool Windows => (modifiers & KeyboardModifiers.Windows) != 0;

        public bool IsMouseLeftDown => source.mouseState.isLeftMouseDown;
        public bool IsMouseLeftDownThisFrame => source.mouseState.isLeftMouseDownThisFrame;
        public bool IsMouseLeftUpThisFrame => source.mouseState.isLeftMouseUpThisFrame;

        public bool IsMouseRightDown => source.mouseState.isRightMouseDown;
        public bool IsMouseRightDownThisFrame => source.mouseState.isRightMouseDownThisFrame;
        public bool IsMouseRightUpThisFrame => source.mouseState.isRightMouseUpThisFrame;
        
        public bool IsMouseMiddleDown => source.mouseState.isMiddleMouseDown;
        public bool IsMouseMiddleDownThisFrame => source.mouseState.isMiddleMouseDownThisFrame;
        public bool IsMouseMiddleUpThisFrame => source.mouseState.isMiddleMouseUpThisFrame;

        public bool IsDoubleClick => source.mouseState.isDoubleClick;
        public bool IsTripleClick => source.mouseState.isTripleClick;
        
        public Vector2 ScrollDelta => source.mouseState.scrollDelta;

        public Vector2 MousePosition => source.mouseState.mousePosition;
        public Vector2 MouseDownPosition => source.mouseState.mouseDownPosition;
    }

}