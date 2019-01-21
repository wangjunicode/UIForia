namespace UIForia {

    public struct TransformOffset {

        public float value;
        public TransformUnit unit;

        public TransformOffset(float value, TransformUnit unit = TransformUnit.Pixel) {
            this.value = value;
            this.unit = unit;
        }
    }

}