namespace Src.Layout {

    public struct GridTrackSizer {

        public GridTrackSizeFn minSizingFunction;
        public GridTrackSizeFn maxSizingFunction;

        public GridTrackSizer(GridTrackSizeFn minSizingFunction, GridTrackSizeFn maxSizingFunction = default(GridTrackSizeFn)) {
            this.minSizingFunction = minSizingFunction;
            this.maxSizingFunction = maxSizingFunction;
        }

    }

}