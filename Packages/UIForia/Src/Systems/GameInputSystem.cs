using UnityEngine;

namespace UIForia.Systems.Input {

    public class GameInputSystem : InputSystem {

        public GameInputSystem(ILayoutSystem layoutSystem, KeyboardInputManager keyboardInputManager) : base(layoutSystem, keyboardInputManager) { }

        private int clickCount;
        private float lastMouseDownTime;
        private Vector2 lastMouseDownPosition;
        private const float k_ClickThreshold = 0.33f;

        protected override MouseState GetMouseState() {
            MouseState retn = new MouseState();
            retn.leftMouseButtonState.isDown = UnityEngine.Input.GetMouseButton(0);
            retn.rightMouseButtonState.isDown = UnityEngine.Input.GetMouseButton(1);
            retn.middleMouseButtonState.isDown = UnityEngine.Input.GetMouseButton(2);

            retn.leftMouseButtonState.isDownThisFrame = UnityEngine.Input.GetMouseButtonDown(0);
            retn.rightMouseButtonState.isDownThisFrame = UnityEngine.Input.GetMouseButtonDown(1);
            retn.middleMouseButtonState.isDownThisFrame = UnityEngine.Input.GetMouseButtonDown(2);

            retn.leftMouseButtonState.isUpThisFrame = UnityEngine.Input.GetMouseButtonUp(0);
            retn.rightMouseButtonState.isUpThisFrame = UnityEngine.Input.GetMouseButtonUp(1);
            retn.middleMouseButtonState.isUpThisFrame = UnityEngine.Input.GetMouseButtonUp(2);
            
            retn.leftMouseButtonState.downPosition = m_MouseState.leftMouseButtonState.downPosition;
            retn.rightMouseButtonState.downPosition = m_MouseState.rightMouseButtonState.downPosition;
            retn.middleMouseButtonState.downPosition = m_MouseState.middleMouseButtonState.downPosition;
            
            retn.leftMouseButtonState.isDrag = m_MouseState.leftMouseButtonState.isDrag;
            retn.rightMouseButtonState.isDrag = m_MouseState.rightMouseButtonState.isDrag;
            retn.middleMouseButtonState.isDrag = m_MouseState.middleMouseButtonState.isDrag;
            retn.mousePosition = ConvertMousePosition(UnityEngine.Input.mousePosition);
            float now = Time.unscaledTime;

            bool didClick = false;

            if (retn.isRightMouseDownThisFrame) {
                retn.rightMouseButtonState.downPosition = retn.mousePosition;
            }

            if (retn.isMiddleMouseDownThisFrame) {
                retn.middleMouseButtonState.downPosition = retn.mousePosition;
            }

            if (retn.isLeftMouseDownThisFrame ) {
                retn.leftMouseButtonState.downPosition = retn.mousePosition;
                lastMouseDownTime = now;
                lastMouseDownPosition = retn.leftMouseButtonState.downPosition;
            }
            else if (retn.isLeftMouseUpThisFrame) {
                if (clickCount == 0 || now - lastMouseDownTime <= k_ClickThreshold) {
                    if (Vector2.Distance(lastMouseDownPosition, retn.mousePosition) <= 3f) {
                        clickCount++;
                        didClick = true;
                    }
                }

                retn.leftMouseButtonState.downPosition = new Vector2(-1, -1);
            }

            if (clickCount > 1 && now - lastMouseDownTime > k_ClickThreshold) {
                clickCount = 0;
            }

            retn.isSingleClick = didClick && clickCount == 1;
            retn.isDoubleClick = didClick && clickCount == 2;
            retn.isTripleClick = didClick && clickCount == 3;
            retn.clickCount = clickCount;
            retn.scrollDelta = UnityEngine.Input.mouseScrollDelta;
            retn.previousMousePosition = m_MouseState.mousePosition;

            return retn;
        }

        private static Vector2 ConvertMousePosition(Vector2 position) {
            return new Vector2(position.x, Screen.height - position.y);
        }
    }
}