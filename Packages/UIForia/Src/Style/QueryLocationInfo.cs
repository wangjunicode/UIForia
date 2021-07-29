namespace UIForia.Style {

    [AssertSize(4)]
    internal struct QueryLocationInfo {

        private uint dataBits;

        private const int sz0 = 24, loc0 = 0, mask0 = ((1 << sz0) - 1) << loc0;
        private const int sz1 = 8, loc1 = loc0 + sz0, mask1 = ((1 << sz1) - 1) << loc1;
        
        public QueryLocationInfo(int queryOffset, int queryCount) {
            this.dataBits = 0;
            this.dataBits = (uint) (dataBits & ~mask0 | (uint)(queryOffset << loc0) & mask0);
            this.dataBits = (uint) (dataBits & ~mask1 | (uint)(queryCount << loc1) & mask1);
        }
        
        public int queryOffset => (int) ((dataBits & mask0) >> loc0);
        
        public int queryCount => (int) (dataBits & mask1) >> loc1;
        
    }

}