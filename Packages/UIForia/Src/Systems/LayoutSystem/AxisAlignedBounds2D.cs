namespace UIForia.Systems {

    public struct AxisAlignedBounds2D {

        public float xMin;
        public float yMin;
        public float xMax;
        public float yMax;

        public AxisAlignedBounds2D(float xMin, float yMin, float xMax, float yMax) {
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;
        }

    }

}