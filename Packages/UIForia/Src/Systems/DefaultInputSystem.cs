using UnityEngine;

namespace UIForia.Systems {

    public class DefaultInputSystem : InputSystem {

        public const float k_DoubleClickDelay = 0.5f;
        public const float k_SingleClickDelay = 0.3f;

        private Vector2 m_LastMouseDownPosition;
        private float m_LastMouseDownTimestamp;
        private bool m_IsDoubleClick;
        private bool m_IsTripleClick;

        public DefaultInputSystem(ILayoutSystem layoutSystem, IStyleSystem styleSystem)
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
            retn.mouseDownPosition = m_MouseState.mouseDownPosition;
            float now = Time.unscaledTime;

            if (retn.isLeftMouseDown || retn.isRightMouseDown) {
                if (retn.isLeftMouseDownThisFrame || retn.isRightMouseDownThisFrame) {
                    retn.mouseDownPosition = ConvertMousePosition(UnityEngine.Input.mousePosition);
                    if (now - m_LastMouseDownTimestamp <= k_DoubleClickDelay && Vector2.Distance(m_LastMouseDownPosition, retn.mouseDownPosition) <= 3f) {
                        if (!m_IsDoubleClick) {
                            m_IsDoubleClick = true;
                        }
                        else {
                            m_IsTripleClick = true;
                        }
                    }
                    m_LastMouseDownPosition = retn.mouseDownPosition;
                    m_LastMouseDownTimestamp = Time.unscaledTime;
                }
            }
            else {
                retn.mouseDownPosition = new Vector2();
            }

            // todo formalize clicking with different buttons
            retn.isSingleClick = (retn.isRightMouseUpThisFrame || retn.isLeftMouseUpThisFrame) && (now - m_LastMouseDownTimestamp < k_SingleClickDelay);

            retn.mousePosition = ConvertMousePosition(UnityEngine.Input.mousePosition);
            retn.scrollDelta = UnityEngine.Input.mouseScrollDelta;
            retn.previousMousePosition = m_MouseState.mousePosition;

            if (now - m_LastMouseDownTimestamp > k_DoubleClickDelay) {
                m_IsDoubleClick = false;
                m_IsTripleClick = false;
                m_LastMouseDownPosition = new Vector2(-999f, -999f);
            }

            // only true on the frame the mouse is down
            retn.isDoubleClick = m_IsDoubleClick;
            retn.isTripleClick = m_IsTripleClick;
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