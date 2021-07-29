using System;
using Unity.Mathematics;

namespace UIForia.UIInput {

    public struct MouseState {

        public MouseButtonState leftMouseButtonState;
        public MouseButtonState middleMouseButtonState;
        public MouseButtonState rightMouseButtonState;

        public float2 mousePosition;
        public float2 previousMousePosition;

        public float2 scrollDelta;

        public bool isDoubleClick;
        public bool isTripleClick;
        public bool isSingleClick;
        public int clickCount;
        public bool isLeftMouseUpThisFrame => leftMouseButtonState.isUpThisFrame;
        public bool isRightMouseUpThisFrame => rightMouseButtonState.isUpThisFrame;
        public bool isMiddleMouseUpThisFrame => middleMouseButtonState.isUpThisFrame;

        public bool isLeftMouseDownThisFrame => leftMouseButtonState.isDownThisFrame;
        public bool isRightMouseDownThisFrame => rightMouseButtonState.isDownThisFrame;
        public bool isMiddleMouseDownThisFrame => middleMouseButtonState.isDownThisFrame;

        public bool isLeftMouseDown => leftMouseButtonState.isDown;
        public bool isRightMouseDown => rightMouseButtonState.isDown;
        public bool isMiddleMouseDown => middleMouseButtonState.isDown;

        public float2 MouseDelta => previousMousePosition - mousePosition;
        
        public bool DidMove {
            get {
                float2 delta =  previousMousePosition - mousePosition;
                return math.distancesq(delta.x, delta.y) > 0;
            }
        }

        public bool AnyMouseDown => leftMouseButtonState.isDown || rightMouseButtonState.isDown || middleMouseButtonState.isDown;
        public bool AnyMouseDownThisFrame => leftMouseButtonState.isDownThisFrame || rightMouseButtonState.isDownThisFrame || middleMouseButtonState.isDownThisFrame;
        public bool ReleasedDrag => leftMouseButtonState.ReleasedDrag || rightMouseButtonState.ReleasedDrag || middleMouseButtonState.ReleasedDrag;

        public float2 MouseDownDelta(MouseButtonType mouseButtonType) {

            MouseButtonState state;

            switch (mouseButtonType) {
                case MouseButtonType.Left:
                    state = leftMouseButtonState;
                    break;

                case MouseButtonType.Middle:
                    state = middleMouseButtonState;
                    break;

                case MouseButtonType.Right:
                    state = rightMouseButtonState;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mouseButtonType), mouseButtonType, null);
            }

            if (state.downPosition.x < 0 || state.downPosition.y < 0) {
                return float2.zero;
            }

            return mousePosition - state.downPosition;
        }

        public float2 MouseDownPosition => leftMouseButtonState.isDown
            ? leftMouseButtonState.downPosition
            : rightMouseButtonState.isDown
                ? rightMouseButtonState.downPosition
                : middleMouseButtonState.isDown
                    ? middleMouseButtonState.downPosition
                    : default;

    }

}