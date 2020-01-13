using JetBrains.Annotations;
using UIForia.Systems;
using UIForia.Systems.Input;
using UIForia.UIInput;
using UnityEngine;

namespace Tests.Mocks {

    public class MockInputSystem : InputSystem {

        public MockInputSystem(ILayoutSystem layoutSystem)
            : base(layoutSystem) { }

        public void SetMouseState(MouseState mouseState) {
            base.mouseState = mouseState;
        }

        // for the debugger, rider struggles w/ partial classes
        [UsedImplicitly]
        public override void OnUpdate() {
            base.OnUpdate();
        }
        
        protected override MouseState GetMouseState() {
            return mouseState;
        }

        public void SetKeyDown(char character, KeyboardModifiers modifiers = 0) {
            ProcessKeyboardEvent(0, InputEventType.KeyDown, character, modifiers);
        }

        public void SetKeyUp(char character, KeyboardModifiers modifiers = 0) {
            ProcessKeyboardEvent(0, InputEventType.KeyUp, character, modifiers);
        }
        
        public void SetMousePosition(Vector2 position) {
            mouseState.previousMousePosition = mouseState.mousePosition;
            mouseState.mousePosition = position;
        }

        public void MouseDragMove(Vector2 position) {
            mouseState.leftMouseButtonState.isDown = true;
            mouseState.leftMouseButtonState.isDownThisFrame = false;
            mouseState.leftMouseButtonState.isUpThisFrame = false;
            mouseState.previousMousePosition = mouseState.mousePosition;
            mouseState.mousePosition = position;
        }
        
        public void MouseDown(Vector2 position) {
            mouseState.leftMouseButtonState.isDown = true;
            mouseState.leftMouseButtonState.isDownThisFrame = true;
            mouseState.leftMouseButtonState.isUpThisFrame = false;
            mouseState.previousMousePosition = mouseState.mousePosition;
            mouseState.mousePosition = position;
            mouseState.leftMouseButtonState.downPosition = position;
        }

        public void MouseUp() {
            mouseState.leftMouseButtonState.isDown = false;
            mouseState.leftMouseButtonState.isDownThisFrame = false;
            mouseState.leftMouseButtonState.isUpThisFrame = true;
            mouseState.leftMouseButtonState.downPosition = new Vector2(-1, -1);
        }

        public void ClearClickState() {
            mouseState.leftMouseButtonState.isDown = false;
            mouseState.leftMouseButtonState.isDownThisFrame = false;
            mouseState.leftMouseButtonState.isUpThisFrame = false;
            mouseState.leftMouseButtonState.downPosition = new Vector2(-1, -1);
        }
        
    }
}