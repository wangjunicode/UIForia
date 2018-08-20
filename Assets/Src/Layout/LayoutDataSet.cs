using UnityEngine;

namespace Src.Layout {

    public struct LayoutDataSet {

        public readonly Rect result;
        public readonly LayoutData data;

        public LayoutDataSet(LayoutData data, Rect result) {
            this.data = data;
            this.result = result;
        }

    }

}