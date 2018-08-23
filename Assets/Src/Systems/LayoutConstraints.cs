using System.Diagnostics;
using Rendering;

namespace Src.Systems {

    [DebuggerDisplay("{minWidth}, {maxWidth}, {minHeight}, {maxHeight}")]
    public struct LayoutConstraints {

        public int growthFactor;
        public int shrinkFactor;

        public UIMeasurement minWidth;
        public UIMeasurement maxWidth;

        public UIMeasurement minHeight;
        public UIMeasurement maxHeight;

        public LayoutConstraints(
            UIMeasurement minWidth,
            UIMeasurement maxWidth,
            UIMeasurement minHeight,
            UIMeasurement maxHeight,
            int growthFactor = 0,
            int shrinkFactor = 0) {
            this.minWidth = minWidth;
            this.maxWidth = maxWidth;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.growthFactor = growthFactor;
            this.shrinkFactor = shrinkFactor;
        }

        public static LayoutConstraints Unset =>
            new LayoutConstraints(
                UIMeasurement.Unset,
                UIMeasurement.Unset,
                UIMeasurement.Unset,
                UIMeasurement.Unset,
                IntUtil.UnsetValue,
                IntUtil.UnsetValue
            );

    }

}