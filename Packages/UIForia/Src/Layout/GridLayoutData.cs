using System;

namespace UIForia.Layout {

    internal unsafe struct GridLayoutData {

        public CheckedArray<GridAxisInfo> gridAxisInfos;
        public CheckedArray<GridItemPlacement> placements;
        public CheckedArray<GridCell> gridCells;
        // public CheckedArray<GridAxisInfo> gridAxisInfos;
        //
        // public GridItemPlacement* placements;
        // public int placementCount;

        // public GridCell* gridCells;
        // public int gridCellCount;

        public GridAxisInfo GetGridInfo(int boxIdx) {

            if (gridAxisInfos.size <= 64) {

                int i = 0;
                while (true) {

                    if (gridAxisInfos[i].boxId == boxIdx) {
                        return gridAxisInfos[i];
                    }

                    i++;
#if DEBUG
                    if (i >= 64) {
                        throw new Exception("Grid Info not found");
                    }
#endif
                }

            }

            int low = 0;
            int high = gridAxisInfos.size - 1;

            while (low <= high) {

                int index = low + (high - low >> 1);
                int gridBoxIdx = gridAxisInfos[index].boxId;

                if (gridBoxIdx == boxIdx) {
                    return gridAxisInfos[index];
                }

                int cmp = boxIdx.CompareTo(gridBoxIdx);

                if (cmp < 0) {
                    low = index + 1;
                }
                else {
                    high = index - 1;
                }
            }
#if DEBUG
            throw new Exception("Grid Info not found");
#endif
            return default;

        }

        public CheckedArray<GridItemPlacement> GetPlacementList() {
            return placements;
        }
        
        public CheckedArray<GridCell> GetCellList() {
            return gridCells;
        }

    }

}