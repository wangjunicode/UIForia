using System.Diagnostics;

namespace UIForia {

    public unsafe struct SelectorQueryDebugView {

        public SelectionSource source;
        public int filterCount;
        public int whereFilterId;
        public SelectorFilter[] filters;
        
        public SelectorQueryDebugView(SelectorQuery target) {
            this.source = target.source;
            this.filterCount = target.filterCount;
            this.whereFilterId = target.whereFilterId;
            this.filters = DebugUtil.PointerListToArray(target.filters, target.filterCount);
        }

    }

    [DebuggerTypeProxy(typeof(SelectorQueryDebugView))]
    public unsafe struct SelectorQuery {

        public SelectionSource source;
        public int filterCount;
        public int whereFilterId;
        public SelectorFilter* filters;

    }

}