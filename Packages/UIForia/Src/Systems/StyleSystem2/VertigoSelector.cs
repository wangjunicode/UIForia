using UIForia.Selectors;

namespace UIForia {
   
    public struct VertigoSelector {

        // currently sizeof(VertigoSelector) == 32 
        public SelectorId id;
        public int queryId; // same as id.index, cannot have selector without query? or allow for mix and match?
        public int propertyOffset;
        public int propertyCount;
        public int eventOffset;
        public int eventCount;
        public FromTarget target;

    }

}