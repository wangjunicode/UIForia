using UnityEngine;

namespace Src.Layout {

    public struct GridPlacement {

        public readonly GridItem colItem;
        public readonly GridItem rowItem;

        public GridPlacement(GridItem rowItem, GridItem colItem) {
            this.colItem = colItem;
            this.rowItem = rowItem;
//            this.rowStart = rowStart;
//            this.rowSpan = Mathf.Max(1, rowSpan);
//            this.colStart = colStart;
//            this.colSpan = Mathf.Max(1, colSpan);
        }
//
//        public int rowEnd => rowStart + rowSpan;
//
//        public int colEnd => colStart + colSpan;

    }

}