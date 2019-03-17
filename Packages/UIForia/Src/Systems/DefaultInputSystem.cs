using UIForia.Stystems.InputSystem;
using UnityEngine;

namespace UIForia.Systems {

    public class DefaultInputSystem : InputSystem {

        public const float k_DoubleClickDelay = 0.5f;
        public const float k_SingleClickDelay = 0.3f;

        private Vector2 m_LastMouseDownPosition;
        private float m_LastMouseDownTimestamp;
        private bool m_IsDoubleClick;
        private bool m_IsTripleClick;

        public DefaultInputSystem(ILayoutSystem layoutSystem)
            : base(layoutSystem) { }

        // todo -- formalize clicking with different buttons
        // todo -- fix double and triple clicking
        
        protected override MouseState GetMouseState() {
            MouseState retn = new MouseState();
            retn.isLeftMouseDown = Input.GetMouseButton(0);
            retn.isRightMouseDown = Input.GetMouseButton(1);
            retn.isMiddleMouseDown = Input.GetMouseButton(2);

            retn.isLeftMouseDownThisFrame = Input.GetMouseButtonDown(0);
            retn.isRightMouseDownThisFrame = Input.GetMouseButtonDown(1);
            retn.isMiddleMouseDownThisFrame = Input.GetMouseButtonDown(2);

            retn.isLeftMouseUpThisFrame = Input.GetMouseButtonUp(0);
            retn.isRightMouseUpThisFrame = Input.GetMouseButtonUp(1);
            retn.isMiddleMouseUpThisFrame = Input.GetMouseButtonUp(2);
            retn.mouseDownPosition = m_MouseState.mouseDownPosition;
            float now = Time.unscaledTime;

            if (retn.isLeftMouseDown || retn.isRightMouseDown) {
                if (retn.isLeftMouseDownThisFrame || retn.isRightMouseDownThisFrame) {
                    retn.mouseDownPosition = ConvertMousePosition(Input.mousePosition);
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
                retn.isSingleClick = (retn.isRightMouseUpThisFrame || retn.isLeftMouseUpThisFrame) && (now - m_LastMouseDownTimestamp < k_SingleClickDelay);
                if (!retn.isSingleClick) {
                    retn.mouseDownPosition = new Vector2();
                }
            }

            // todo formalize clicking with different buttons
            
            retn.mousePosition = ConvertMousePosition(Input.mousePosition);
            retn.scrollDelta = Input.mouseScrollDelta;
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