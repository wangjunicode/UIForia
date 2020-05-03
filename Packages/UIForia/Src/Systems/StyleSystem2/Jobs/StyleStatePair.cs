
namespace UIForia {

    [AssertSize(8)]
    public struct StyleStatePair {

        public readonly StyleId styleId;
        public readonly StyleState2 state;

        public StyleStatePair(StyleId styleId, StyleState2 state ) {
            this.styleId = styleId;
            this.state = state;
        }


    }


}