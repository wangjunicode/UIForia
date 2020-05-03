namespace UIForia {
    
    public struct VertigoStyle {

        //  currently sizeof(VertigoStyle) == 32 bytes, perfect cache line fit
        public readonly StyleId id;
        public readonly ushort propertyOffset;
        public readonly ushort normalCount;
        public readonly ushort hoverCount;
        public readonly ushort focusCount;
        public readonly ushort activeCount;
        public readonly ushort selectorOffset;
        public readonly ushort selectorCount;
        public readonly ushort eventOffset;
        public readonly ushort eventCount;

        internal VertigoStyle(StyleId id, ushort propertyOffset, ushort normalCount, ushort hoverCount, ushort focusCount, ushort activeCount, ushort selectorOffset, ushort selectorCount, ushort eventOffset, ushort eventCount) {
            this.id = id;
            this.propertyOffset = propertyOffset;
            this.normalCount = normalCount;
            this.activeCount = activeCount;
            this.focusCount = focusCount;
            this.hoverCount = hoverCount;
            this.selectorOffset = selectorOffset;
            this.selectorCount = selectorCount;
            this.eventOffset = eventOffset;
            this.eventCount = eventCount;
        }

        public int index {
            get => id.index;
        }
        
        public int GetPropertyStart(StyleState2 state) {
            switch (state) {

                case StyleState2.Normal:
                    return propertyOffset;

                case StyleState2.Hover:
                    return propertyOffset + normalCount;

                case StyleState2.Focused:
                    return propertyOffset + normalCount + hoverCount;

                case StyleState2.Active:
                    return propertyOffset + normalCount + hoverCount + focusCount;

            }

            return -1;
        }

        public int GetPropertyCount(StyleState2 state) {
            switch (state) {

                case StyleState2.Normal:
                    return normalCount;

                case StyleState2.Hover:
                    return hoverCount;

                case StyleState2.Focused:
                    return focusCount;

                case StyleState2.Active:
                    return activeCount;

            }

            return 0;
        }

    }

}