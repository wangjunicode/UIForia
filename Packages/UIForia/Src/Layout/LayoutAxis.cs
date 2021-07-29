
using UIForia.Util.Unsafe;

namespace UIForia.Layout {

    internal struct LayoutAxis {

        public CheckedArray<float> borderStart;
        public CheckedArray<float> borderEnd;
        
        public CheckedArray<float> marginSize;
        
        public CheckedArray<SolvedSize> solvedPrefSizes;
        public CheckedArray<SolvedConstraint> solvedMinSizes;
        public CheckedArray<SolvedConstraint> solvedMaxSizes;
        
        public CheckedArray<float> paddingBorderStart;
        public CheckedArray<float> paddingBorderEnd;
        public CheckedArray<ResolvedSpacerSize> marginStart;
        public CheckedArray<ResolvedSpacerSize> marginEnd;
        public CheckedArray<ResolvedSpacerSize> spaceBetween;
        public CheckedArray<ResolvedSpacerSize> paddingStart;
        public CheckedArray<ResolvedSpacerSize> paddingEnd;
        
        public CheckedArray<ReadyFlags> readyFlags;
        
        public CheckedArray<float> outputSizes;
        public CheckedArray<float> outputPositions;
        
        public CheckedArray<LayoutSizes> sizes;
        public CheckedArray<LayoutContentSizes> contentSizes;
        
        public bool isVertical; // todo -- find better home for this?
        
        public DataList<InterpolatedStyleValue> animatedSizes;

    }

}