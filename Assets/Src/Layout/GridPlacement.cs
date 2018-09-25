using System.Diagnostics;

namespace Src.Layout {

    [DebuggerDisplay("({colItem.trackStart}, {colItem.trackSpan}), ({rowItem.trackStart}, rowItem.trackSpan})")]
    public struct GridPlacement {

        public readonly GridItem colItem;
        public readonly GridItem rowItem;

        public GridPlacement(GridItem rowItem, GridItem colItem) {
            this.colItem = colItem;
            this.rowItem = rowItem;
        }

    }

}