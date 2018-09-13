using UnityEngine;

namespace Src.Systems {

    public class GOInputSystem : InputSystem {

        public GOInputSystem(ILayoutSystem layoutSystem, IStyleSystem styleSystem)
            : base(layoutSystem, styleSystem) { }

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
                        
            if (retn.isLeftMouseDown) {
                if (retn.isLeftMouseDownThisFrame) {
                    retn.mouseDownPosition = ConvertMousePosition(UnityEngine.Input.mousePosition);
                }
            }
            else {
                retn.mouseDownPosition = new Vector2(-1, -1);
            }

            retn.mousePosition = ConvertMousePosition(UnityEngine.Input.mousePosition);
            retn.scrollDelta = UnityEngine.Input.mouseScrollDelta;
            retn.previousMousePosition = m_MouseState.mousePosition;
            return retn;
        }

        private static Vector2 ConvertMousePosition(Vector2 position) {
            return new Vector2(position.x, Screen.height - position.y);
        }
    }

}