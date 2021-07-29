using System;
using UnityEngine;

namespace UIForia.Layout {

    internal struct FinalPassSize {

        public float value;
        public int stretch;
        public FinalPassSizeType sizeType;

    }

    internal enum FinalPassSizeType {

        Pixel,
        StretchContent,
        FitContent,
        Controlled

    }

    public struct InitialPassSize {

        public float value;
        public InitialPassSizeUnit unit;

    }
     internal struct LayoutInfo2 {

         public int boxIndex;
         public RangeInt childRange;

     }
    [Flags]
    public enum InitialPassSizeUnit {

        // for the hug pass we only have pixel and content
        // everything else is collapsed because there is no available space to grow
        // we pre-compute min and max child sizes
        // we post-compute controlled sizes
        // stretches are 0 (no extra space)
        // percent is 0 (no extra space)
        Pixel = 1 << 0,
        Content = 1 << 1,

    }

    internal struct InitialAxis {

        public CheckedArray<float> marginSize;
        public CheckedArray<float> pendingSize;
        public CheckedArray<float> pendingMinSize; // maybe we can get rid of this and roll it into a single pending size that is pre-clamped
        public CheckedArray<float> pendingMaxSize; // maybe we can get rid of this and roll it into a single pending size that is pre-clamped

        public CheckedArray<InitialPassSize> prefSize;
        public CheckedArray<InitialPassSize> minSize;
        public CheckedArray<InitialPassSize> maxSize;

        public CheckedArray<float> resolvedSizes;
        public CheckedArray<SolvedSize> solvedPrefSizes;
        public CheckedArray<SolvedConstraint> solvedMinSizes;
        public CheckedArray<SolvedConstraint> solvedMaxSizes;

        public CheckedArray<float> paddingBorderStart;
        public CheckedArray<float> paddingBorderEnd;
        public CheckedArray<ResolvedSpacerSize> marginStart;
        public CheckedArray<ResolvedSpacerSize> marginEnd;
        public CheckedArray<ResolvedSpacerSize> spaceBetween;

        public CheckedArray<SizeTypeFlags> sizeFlags;
        public CheckedArray<int> indexToParentIndex;

    }

}