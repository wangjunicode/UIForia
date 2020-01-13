using UnityEngine;

namespace UIForia.Systems.Input {

    public class GameInputSystem : InputSystem {

        public GameInputSystem(ILayoutSystem layoutSystem) : base(layoutSystem) { }

        private int clickCount;
        private float lastMouseDownTime;
        private Vector2 lastMouseDownPosition;
        
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
            
            retn.leftMouseButtonState.downPosition = mouseState.leftMouseButtonState.downPosition;
            retn.rightMouseButtonState.downPosition = mouseState.rightMouseButtonState.downPosition;
            retn.middleMouseButtonState.downPosition = mouseState.middleMouseButtonState.downPosition;
            
            retn.leftMouseButtonState.isDrag = mouseState.leftMouseButtonState.isDrag;
            retn.rightMouseButtonState.isDrag = mouseState.rightMouseButtonState.isDrag;
            retn.middleMouseButtonState.isDrag = mouseState.middleMouseButtonState.isDrag;
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
                if (Vector2.Distance(lastMouseDownPosition, retn.mousePosition) <= 3f) {
                    clickCount++;
                    didClick = true;
                }
                
                retn.leftMouseButtonState.downPosition = new Vector2(-1, -1);
            }

            retn.isSingleClick = didClick && clickCount == 1;
            retn.isDoubleClick = didClick && clickCount == 2;
            retn.isTripleClick = didClick && clickCount == 3;
            retn.clickCount = clickCount;
            retn.scrollDelta = UnityEngine.Input.mouseScrollDelta;
            retn.previousMousePosition = mouseState.mousePosition;

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