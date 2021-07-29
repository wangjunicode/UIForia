using UIForia.UIInput;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia {

    public struct TouchState {

        

    }

    public class MouseAdapter {

        public float clickDistanceThreshold = 3f;
        public float clickThresholdSeconds = 0.33f;

        protected const float k_ClickDistanceThreshold = 3f;
        protected const float k_ClickThresholdSeconds = 0.33f;

        protected int clickCount;
        protected float lastMouseDownTime;
        protected float2 lastMouseDownPosition;
        protected float2 unsetDownPosition = new float2(-1, -1);

        protected MouseState mouseState;

        public virtual MouseState GetMouseState(float dpiScaleFactor, Size appSize) {
            MouseState retn = new MouseState();

            if (clickDistanceThreshold <= 0) clickDistanceThreshold = k_ClickDistanceThreshold;
            if (clickThresholdSeconds <= 0) clickThresholdSeconds = k_ClickThresholdSeconds;

            retn.leftMouseButtonState.isDown = Input.GetMouseButton(0);
            retn.rightMouseButtonState.isDown = Input.GetMouseButton(1);
            retn.middleMouseButtonState.isDown = Input.GetMouseButton(2);

            retn.leftMouseButtonState.isDownThisFrame = Input.GetMouseButtonDown(0);
            retn.rightMouseButtonState.isDownThisFrame = Input.GetMouseButtonDown(1);
            retn.middleMouseButtonState.isDownThisFrame = Input.GetMouseButtonDown(2);

            retn.leftMouseButtonState.isUpThisFrame = Input.GetMouseButtonUp(0);
            retn.rightMouseButtonState.isUpThisFrame = Input.GetMouseButtonUp(1);
            retn.middleMouseButtonState.isUpThisFrame = Input.GetMouseButtonUp(2);

            retn.leftMouseButtonState.downPosition = mouseState.leftMouseButtonState.downPosition;
            retn.rightMouseButtonState.downPosition = mouseState.rightMouseButtonState.downPosition;
            retn.middleMouseButtonState.downPosition = mouseState.middleMouseButtonState.downPosition;

            retn.leftMouseButtonState.isDrag = mouseState.leftMouseButtonState.isDrag;
            retn.rightMouseButtonState.isDrag = mouseState.rightMouseButtonState.isDrag;
            retn.middleMouseButtonState.isDrag = mouseState.middleMouseButtonState.isDrag;

            retn.mousePosition = ConvertMousePosition(Input.mousePosition, dpiScaleFactor, appSize.height);

            float now = Time.unscaledTime;

            bool didClick = false;
            
            if (clickCount > 0 && now - lastMouseDownTime > clickThresholdSeconds) {
                clickCount = 0;
            }

            if (retn.isRightMouseDownThisFrame) {
                retn.rightMouseButtonState.downPosition = retn.mousePosition;
            }

            if (retn.isMiddleMouseDownThisFrame) {
                retn.middleMouseButtonState.downPosition = retn.mousePosition;
            }

            if (retn.isLeftMouseDownThisFrame) {
                retn.leftMouseButtonState.downPosition = retn.mousePosition;
                lastMouseDownTime = now;
                lastMouseDownPosition = retn.leftMouseButtonState.downPosition;
            }

            if (retn.isLeftMouseUpThisFrame) {
                if (clickCount == 0 || now - lastMouseDownTime <= clickThresholdSeconds) {
                    if (math.distance(lastMouseDownPosition, retn.mousePosition) <= clickDistanceThreshold / dpiScaleFactor) {
                        clickCount++;
                        didClick = true;
                    }
                }

                if (!retn.isLeftMouseDownThisFrame) {
                    retn.leftMouseButtonState.downPosition = unsetDownPosition;
                }
            }

            retn.isSingleClick = didClick && clickCount == 1;
            retn.isDoubleClick = didClick && clickCount == 2;
            retn.isTripleClick = didClick && clickCount == 3;
            retn.clickCount = clickCount;
            retn.scrollDelta = Input.mouseScrollDelta;
            retn.previousMousePosition = mouseState.mousePosition;

            mouseState = retn;

            return retn;
        }

        public static float2 ConvertMousePosition(Vector2 position, float dpiScaleFactor, float appHeight) {
            float2 scaledPosition = position / dpiScaleFactor;
            float scaledHeight = appHeight / dpiScaleFactor;
            return new float2(scaledPosition.x, scaledHeight - scaledPosition.y);
        }

        public virtual float2 GetMousePosition() {
            throw new System.NotImplementedException();
        }

    }

}