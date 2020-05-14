namespace UIForia {

    [AssertSize(32)]
    public struct ActiveSelector {

        public ElementId elementId;
        public int selectorIndex;
        public ListHandle targets;
        public int padding;
        public int padding1;

    }

}