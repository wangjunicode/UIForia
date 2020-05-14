namespace UIForia {

    [AssertSize(16)]
    public struct StyleStateElementId {

        public ElementId elementId;
        public StyleId styleId;
        public StyleState2 state;
        private readonly int padding;

        public StyleStateElementId(StyleId styleId, StyleState2 state, ElementId elementId) {
            this.styleId = styleId;
            this.state = state;
            this.elementId = elementId;
            this.padding = 0;
        }

    }

}