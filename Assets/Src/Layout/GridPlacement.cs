using System.Diagnostics;

namespace Src.Layout {

    [DebuggerDisplay("(id = {id} ({colItem.trackStart}, {colItem.trackSpan}), ({rowItem.trackStart}, rowItem.trackSpan})")]
    public struct GridPlacement {

        public readonly int id;
        public readonly int index;
        public readonly GridItem colItem;
        public readonly GridItem rowItem;

        public GridPlacement(int id, int index, GridItem rowItem, GridItem colItem) {
            this.id = id;
            this.index = index;
            this.colItem = colItem;
            this.rowItem = rowItem;
        }

        public bool IsAxisLocked => false;
        public bool IsAutoPlaced => false;

    }

}