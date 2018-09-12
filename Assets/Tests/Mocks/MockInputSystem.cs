using Src.Systems;
using UnityEngine;

namespace Tests.Mocks {

    public class MockInputSystem : InputSystem {

        public MockInputSystem(ILayoutSystem layoutSystem, IStyleSystem styleSystem)
            : base(layoutSystem, styleSystem) { }

        public void SetMouseState(MouseState mouseState) {
            m_MouseState = mouseState;
        }
        
        protected override MouseState GetMouseState() {
            return m_MouseState;
        }

        public void SetMousePosition(Vector2 position) {
            m_MouseState.previousMousePosition = m_MouseState.mousePosition;
            m_MouseState.mousePosition = position;
        }
    }
}