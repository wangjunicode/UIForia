namespace UIForia.Layout {

    internal struct FinalAxis {

        public CheckedArray<float> outputSize;
        public CheckedArray<FinalPassSize> finalPassSize;
        public CheckedArray<ResolvedSpacerSize> marginStart;
        public CheckedArray<ResolvedSpacerSize> marginEnd;
        public CheckedArray<ResolvedSpacerSize> paddingStart;
        public CheckedArray<ResolvedSpacerSize> paddingEnd;
        public CheckedArray<ResolvedSpacerSize> betweenSpacer;
        public CheckedArray<float> outputPosition;
        public CheckedArray<float> borderStart;
        public CheckedArray<float> borderEnd;
        public bool isVertical;
        public bool fillFromEnd;
        public CheckedArray<SpaceCollapse> spaceCollapse;

    }

}