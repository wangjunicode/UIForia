using UnityEngine;

namespace Src.Layout {

    public struct GridStartEnd {

        public readonly int start;
        public readonly int end;
        
        public GridStartEnd(int start, int end = 0) {
            this.start = start;
            this.end = Mathf.Max(start + 1, end);
        }

    }
    
    public struct GridPlacementParameters {

        public int rowStart;
        public int rowSpan;
        public int colStart;
        public int colSpan;

        public GridPlacementParameters(GridStartEnd col, GridStartEnd row) {
            this.colStart = col.start - 1;
            this.colSpan = col.end - col.start;
            this.rowStart = row.start - 1;
            this.rowSpan = row.end - row.start;
        }
        
        public GridPlacementParameters(int colStart, int rowStart) {
            this.colStart = colStart;
            this.rowStart = rowStart;
            this.colSpan = 1;
            this.rowSpan = 1;
        }
        
        public GridPlacementParameters(int colStart, int colSpan, int rowStart, int rowSpan) {
            this.colStart = colStart;
            this.colSpan = colSpan;
            this.rowStart = rowStart;
            this.rowSpan = rowSpan;
        }

    }

}