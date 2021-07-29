using Unity.Mathematics;

namespace UIForia.UIInput {

    public struct MouseButtonState {

        public MouseButtonType mouseButtonType;
        public bool isDrag;
        public bool isUp;
        public bool isDown;
        public bool isUpThisFrame;
        public bool isDownThisFrame;
        public float downTimestamp;
        public float2 downPosition;
        public float2 upPosition;
        public int clickCount;

        public bool ReleasedDrag => isDrag && !isDown;

    }

}