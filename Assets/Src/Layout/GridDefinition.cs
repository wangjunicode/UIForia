namespace Src.Layout {

    public struct GridDefinition {

        public GridTrackSizer[] rowTemplate;
        public GridTrackSizer[] colTemplate;
        public GridTrackSizer autoRowSize;
        public GridTrackSizer autoColSize;
        public GridAutoPlaceDensity gridFillDensity;
        public GridAutoFlow autoFlow;
        public float rowGap; // todo -- UIMeasurement
        public float colGap; // todo -- UIMeasurement

    }

}