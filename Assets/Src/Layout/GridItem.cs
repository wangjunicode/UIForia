using Rendering;

namespace Src.Layout {

    public class GridItem {

        public int trackStart;
        public int trackSpan;
        public float outputSize;
        public bool spansFlexible;

        public GridItem() { }

        public GridItem(int trackStart, int trackSpan, float preferredSize, bool spansFlexible) {
            this.trackStart = trackStart;
            this.trackSpan = trackSpan;
            this.outputSize = preferredSize;
            this.spansFlexible = spansFlexible;
        }

        public bool IsAxisLocked => IntUtil.IsDefined(trackStart);
        
        public void Reset() {
            trackStart = IntUtil.UnsetValue;
            trackSpan = 1;
            outputSize = 0;
            spansFlexible = false;
        }

    }

}