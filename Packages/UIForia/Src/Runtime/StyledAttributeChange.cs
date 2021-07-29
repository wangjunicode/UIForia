namespace UIForia {

    internal struct StyledAttributeChange {

        public int tagId;
        public ElementId elementId;
        public StyledAttributeOperation operation;

    }

    internal enum StyledAttributeOperation {

        Add,
        Remove,

    }
}