namespace SVGX {

    public struct ScissorRect {

        public bool active;
        public float x;
        public float y;
        public float width;
        public float height;

        public ScissorRect(bool active, float x, float y, float width, float height) {
            this.active = active;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public static ScissorRect Disabled => new ScissorRect(
            false,
            float.MinValue,
            float.MinValue,
            float.MaxValue,
            float.MaxValue
        );

    }

}