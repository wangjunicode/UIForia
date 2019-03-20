using UnityEngine;

namespace UIForia.Systems.Input {

    public class GameInputSystem2 : InputSystem {

        public GameInputSystem2(ILayoutSystem layoutSystem) : base(layoutSystem) { }

        private int clickCount;
        private float lastMouseDownTime;
        private Vector2 lastMouseDownPosition;
        private const float k_ClickThreshold = 0.33f;

        protected override MouseState GetMouseState() {
            MouseState retn = new MouseState();
            retn.isLeftMouseDown = UnityEngine.Input.GetMouseButton(0);
            retn.isRightMouseDown = UnityEngine.Input.GetMouseButton(1);
            retn.isMiddleMouseDown = UnityEngine.Input.GetMouseButton(2);

            retn.isLeftMouseDownThisFrame = UnityEngine.Input.GetMouseButtonDown(0);
            retn.isRightMouseDownThisFrame = UnityEngine.Input.GetMouseButtonDown(1);
            retn.isMiddleMouseDownThisFrame = UnityEngine.Input.GetMouseButtonDown(2);

            retn.isLeftMouseUpThisFrame = UnityEngine.Input.GetMouseButtonUp(0);
            retn.isRightMouseUpThisFrame = UnityEngine.Input.GetMouseButtonUp(1);
            retn.isMiddleMouseUpThisFrame = UnityEngine.Input.GetMouseButtonUp(2);
            retn.mouseDownPosition = m_MouseState.mouseDownPosition;
            retn.mousePosition = ConvertMousePosition(UnityEngine.Input.mousePosition);
            float now = Time.unscaledTime;

            bool didClick = false;

            if (retn.isLeftMouseDownThisFrame) {
                retn.mouseDownPosition = retn.mousePosition;
                lastMouseDownTime = now;
                lastMouseDownPosition = retn.mouseDownPosition;
            }
            else if (retn.isLeftMouseUpThisFrame) {
                if (now - lastMouseDownTime <= k_ClickThreshold) {
                    if (Vector2.Distance(lastMouseDownPosition, retn.mousePosition) <= 3f) {
                        clickCount++;
                        didClick = true;
                    }
                }
                retn.mouseDownPosition = new Vector2(-1, -1);
            }

            if (now - lastMouseDownTime > k_ClickThreshold) {
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
            // todo -- HACK I have no idea why but unity reports the mouse position as 4 pixels different in the editor but apparently not in 4k res
#if UNITY_EDITOR
            return new Vector2(position.x, Screen.height - position.y);
#else
            return new Vector2(position.x, Screen.height - position.y);
#endif
        }

    }

}