namespace UIForia {

    public struct ElementReference {

        internal readonly int index;
        internal readonly int elementId;

        internal ElementReference(int elementId, int index) {
            this.elementId = elementId;
            this.index = index;
        }

    }

    public struct ElementReference<T> {

        internal readonly int index;
        internal readonly int elementId;

        internal ElementReference(int elementId, int index) {
            this.elementId = elementId;
            this.index = index;
        }

    }

}