using System.Collections.Generic;
using Src.Util;

namespace Src.Layout {

    public class GridTrack {

        public readonly GridTrackSizer sizingFn;
        public float offset;
        public float baseSize;
        public float growthLimit;
        public List<GridItem> originMembers;
        public bool isOccupied;

        public GridTrack(GridTrackSizer sizingFn) {
            this.sizingFn = sizingFn;
            this.originMembers = ListPool<GridItem>.Get();
            this.offset = 0;
            this.baseSize = 0;
            this.growthLimit = 0;
        }

        public List<GridItem> GetItemsWithSpan(int spanSize) {
            List<GridItem> retn = ListPool<GridItem>.Get();
            for (int i = 0; i < originMembers.Count; i++) {
                if (!originMembers[i].spansFlexible && originMembers[i].trackSpan == spanSize) {
                    retn.Add(originMembers[i]);
                }
            }

            return retn;
        }

    }

}