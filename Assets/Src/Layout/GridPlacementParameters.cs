namespace Src.Layout {

    public struct GridPlacementParameters {

        public int rowStart;
        public int rowSpan;
        public int colStart;
        public int colSpan;

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