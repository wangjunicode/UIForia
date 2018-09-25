using Rendering;

namespace Src.Layout {

    public class GridItem {

        public int trackStart;
        public int trackSpan;
        public float minSize;
        public float outputSize;
        public float maxSize;
        public bool spansFlexible;

        public GridItem() { }

        public GridItem(int trackStart, int trackSpan, float minSize, float maxSize, float preferredSize, bool spansFlexible) {
            this.trackStart = trackStart;
            this.trackSpan = trackSpan;
            this.outputSize = preferredSize;
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.spansFlexible = spansFlexible;
        }

        public bool IsAxisLocked => IntUtil.IsDefined(trackStart);
        
        public void Reset() {
            trackStart = IntUtil.UnsetValue;
            trackSpan = 1;
            outputSize = 0;
            minSize = 0;
            maxSize = 0;
            spansFlexible = false;
        }

    }

}