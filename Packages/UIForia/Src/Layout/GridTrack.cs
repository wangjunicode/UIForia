using System.Collections.Generic;
using UIForia.Layout.LayoutTypes;
using UIForia.Util;

namespace UIForia.Layout {

    public struct GridTrack {

        public float position;
        public float outputSize;
        public float growthLimit;
        public GridTrackSize size;
        public List<int> spanningItems;
        public int autoPlacementCursor;
        public bool skipAutoplacement;
        
        public GridTrack(GridTrackSize size) {
            this.size = size;
            this.position = 0;
            this.outputSize = 0;
            this.growthLimit = 0;
            this.autoPlacementCursor = 0;
            this.skipAutoplacement = false;
            this.spanningItems = ListPool<int>.Get();
        }

        public float End => position + outputSize;

    }

}