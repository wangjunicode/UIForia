using JetBrains.Annotations;
using UIForia.Systems;
using UIForia.Systems.Input;
using UnityEngine;

namespace Tests.Mocks {

    public class MockInputSystem : InputSystem {
        
        public MockInputSystem(ILayoutSystem layoutSystem) : base(layoutSystem, new MockKeyboardInputManager()) { }

        public void SetMouseState(MouseState mouseState) {
            m_MouseState = mouseState;
        }

        // for the debugger, rider struggles w/ partial classes
        [UsedImplicitly]
        public override void OnUpdate() {
            base.OnUpdate();
        }
        
        protected override MouseState GetMouseState() {
            return m_MouseState;
        }

        public void SetMousePosition(Vector2 position) {
            m_MouseState.previousMousePosition = m_MouseState.mousePosition;
            m_MouseState.mousePosition = position;
        }

        public void MouseDragMove(Vector2 position) {
            m_MouseState.leftMouseButtonState.isDown = true;
            m_MouseState.leftMouseButtonState.isDownThisFrame = false;
            m_MouseState.leftMouseButtonState.isUpThisFrame = false;
            m_MouseState.previousMousePosition = m_MouseState.mousePosition;
            m_MouseState.mousePosition = position;
        }
        
        public void MouseDown(Vector2 position) {
            m_MouseState.leftMouseButtonState.isDown = true;
            m_MouseState.leftMouseButtonState.isDownThisFrame = true;
            m_MouseState.leftMouseButtonState.isUpThisFrame = false;
            m_MouseState.previousMousePosition = m_MouseState.mousePosition;
            m_MouseState.mousePosition = position;
            m_MouseState.leftMouseButtonState.downPosition = position;
        }

        public void MouseUp() {
            m_MouseState.leftMouseButtonState.isDown = false;
            m_MouseState.leftMouseButtonState.isDownThisFrame = false;
            m_MouseState.leftMouseButtonState.isUpThisFrame = true;
            m_MouseState.leftMouseButtonState.downPosition = new Vector2(-1, -1);
        }

        public void ClearClickState() {
            m_MouseState.leftMouseButtonState.isDown = false;
            m_MouseState.leftMouseButtonState.isDownThisFrame = false;
            m_MouseState.leftMouseButtonState.isUpThisFrame = false;
            m_MouseState.leftMouseButtonState.downPosition = new Vector2(-1, -1);
        }

        public void MouseMove(Vector2 position) {
            
        }
    }

    public class MockKeyboardInputManager : KeyboardInputManager {

        public KeyboardInputState inputState;

        public MockKeyboardInputManager() {
            inputState = new KeyboardInputState();
        }

        public void ResetInputState() {
            inputState = new KeyboardInputState();
        }

        public override KeyboardInputState UpdateKeyboardInputState() {
            return inputState;
        }
    }
}