namespace UIForia {

    [AssertSize(32)]
    public unsafe struct ResolvedSelectorFilter {

        public SelectorFilterType filterType;
        public int indexTableSize;
        public int key;
        public int value;
        public int* indexTable;
        
        public int padding0;
        public int padding1;

    }

}