using System.Collections.Generic;
using Src.Layout.LayoutTypes;
using Src.Util;

namespace Src.Layout {

    public struct GridTrack {

        public float position;
        public float outputSize;
        public float growthLimit;
        public GridTrackSize size;
        public readonly List<int> spanningItems;

        public GridTrack(GridTrackSize size) {
            this.size = size;
            this.position = 0;
            this.outputSize = 0;
            this.growthLimit = 0;
            this.spanningItems = ListPool<int>.Get();
        }

        public float End => position + outputSize;

    }

}