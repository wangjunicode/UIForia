namespace UIForia {

    public enum StylePairUpdateType {

        Add,
        Remove,
        Current

    }
    
    public unsafe struct StylePairUpdate {

        public readonly int stylePairCount;
        public readonly ElementId elementId;
        public readonly StylePairUpdateType updateType;
        public readonly StyleStatePair* styleStatePairs;

        public StylePairUpdate(ElementId elementId, StylePairUpdateType updateType, StyleStatePair* styleStatePairs, int stylePairCount) {
            this.elementId = elementId;
            this.updateType = updateType;
            this.stylePairCount = stylePairCount;
            this.styleStatePairs = styleStatePairs;
        }

    }

}