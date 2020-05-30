namespace UIForia {

    // [AssertSize(32)]
    public unsafe struct SelectorRunInfo {

        public int selectorId;
        public ElementId hostId;
        public int whereFilterId;
        public int candidateCount;
        public int filterCount;
        public ElementId* candidates;
        public ResolvedSelectorFilter* filters;

    }

}