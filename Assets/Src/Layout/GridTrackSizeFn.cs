namespace Src.Layout {

    public struct GridTrackSizeFn {

        public GridTrackSizeType type;
        public float value;

        public GridTrackSizeFn(GridTrackSizeType type) {
            this.type = type;
            this.value = 1;
        }
        
        public GridTrackSizeFn(GridTrackSizeType type, float value) {
            this.type = type;
            this.value = value;
        }

    }

}