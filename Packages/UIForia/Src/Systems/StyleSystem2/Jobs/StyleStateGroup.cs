namespace UIForia {

    public unsafe struct StyleStateGroup {

        public readonly int index;
        public readonly StyleId styleId;
        public readonly StyleState2 state;
        public readonly int styleSetId;

        public StyleStateGroup(int index, StyleState2 state, StyleId styleId, int styleSetId) {
            this.index = index;
            this.state = state;
            this.styleId = styleId;
            this.styleSetId = styleSetId;
        }

    }

}