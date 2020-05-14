namespace UIForia {

    [AssertSize(16)]
    public struct SelectorMatch {

        public int selectorId;
        public ElementId targetElementId;
        public ElementId sourceElementId;
        private int padding;

    }

}