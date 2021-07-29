using UIForia.Elements;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.UIInput {

    public struct MouseInputEvent {

        public readonly InputEventType type;
        public readonly KeyboardModifiers modifiers;

        private readonly UIForiaRuntimeSystem source;
        public readonly ElementId elementId;
        private int frameId; // use this to make sure the evt is valid 
        
        internal MouseInputEvent(UIForiaRuntimeSystem source, InputEventType type, KeyboardModifiers modifiers, bool isFocused, ElementId elementId) {
            this.source = source;
            this.type = type;
            this.modifiers = modifiers;
            this.elementId = elementId;
            this.frameId = source.currentFrameId;
        }

        public void Consume() {
            if (frameId == source.currentFrameId) {
                source.consumedEvents |= type;
            }
        }

        public void StopPropagation() {
            if (frameId == source.currentFrameId) {
                source.consumedEvents |= type;
            }
        }

        public bool IsConsumed {
            get {
                return frameId != source.currentFrameId || (source.consumedEvents & type) != 0; 
            }
        }

        public bool Alt => (modifiers & KeyboardModifiers.Alt) != 0;

        public bool Shift => (modifiers & KeyboardModifiers.Shift) != 0;

        public bool Ctrl => (modifiers & KeyboardModifiers.Control) != 0;

        public bool OnlyControl => Ctrl && !Alt && !Shift;

        public bool Command => (modifiers & KeyboardModifiers.Command) != 0;

        public bool NumLock => (modifiers & KeyboardModifiers.NumLock) != 0;

        public bool CapsLock => (modifiers & KeyboardModifiers.CapsLock) != 0;

        public bool Windows => (modifiers & KeyboardModifiers.Windows) != 0;

        // todo -- probably wants to either ensure source is at same frame or copy this data
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

        public float2 ScrollDelta => source.mouseState.scrollDelta;

        public float2 MousePosition => source.mouseState.mousePosition;
        public float2 MousePositionInvertY => new float2(source.mouseState.mousePosition.x, source.application.Height - source.mouseState.mousePosition.y);
        public float2 LeftMouseDownPosition => source.mouseState.leftMouseButtonState.downPosition;
        public float2 RightMouseDownPosition => source.mouseState.rightMouseButtonState.downPosition;
        public float2 MiddleMouseDownPosition => source.mouseState.middleMouseButtonState.downPosition;
        public float2 DragDelta => source.mouseState.MouseDelta;

    }

}