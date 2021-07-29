namespace UIForia.Layout {

    internal struct MarginCollapseInfo {

        public CheckedArray<ResolvedSpacerSize> marginStart;
        public CheckedArray<ResolvedSpacerSize> marginEnd;
        public CheckedArray<ResolvedSpacerSize> paddingStart;
        public CheckedArray<ResolvedSpacerSize> paddingEnd;

        public CheckedArray<ResolvedSpacerSize> betweenSpacer;
        public CheckedArray<SpaceCollapse> spaceCollapse;

    }

}