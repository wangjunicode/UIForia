namespace Src.Layout {

    public struct GridTrackSizeFn {

        public GridCellMeasurementType type;
        public float value;

        public GridTrackSizeFn(GridCellMeasurementType type, float value) {
            this.type = type;
            this.value = value;
        }

    }

}