using UnityEngine;

namespace UIForia.Layout {

    internal struct LayoutNode {

        public int parentIndex;
        public ElementId elementId;
        public RangeInt childRange;
        public LayoutBoxType layoutBoxType;

    }

}