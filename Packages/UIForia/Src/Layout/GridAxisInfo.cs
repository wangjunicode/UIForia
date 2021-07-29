using UnityEngine;

namespace UIForia.Layout {

    internal struct GridAxisInfo {

        public int boxId;
        public int cellOffset;
        public ushort cellCount; // ushort for packing reasons
        public bool hasIntrinsicSizes;
        public RangeInt placementRange;
        public RangeInt childRange;

    }

}