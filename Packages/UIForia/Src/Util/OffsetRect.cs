namespace UIForia.Util {

    public struct OffsetRect {

        public readonly float top;
        public readonly float right;
        public readonly float bottom;
        public readonly float left;
        
        public OffsetRect(float top, float right, float bottom, float left) {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }

        public float Horizontal => left + right;

        public float Vertical => top + bottom;

    }

}