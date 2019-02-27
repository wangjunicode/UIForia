namespace UIForia.Rendering {

    public partial class UIStyleSet {

        private struct StyleEntry {

            public readonly UIStyle style;
            public readonly StyleState state;
            public readonly StyleType type;
            public readonly int priority;
            public readonly UIStyleGroup sourceGroup;

            //style number is used to prioritize shared styles, higher numbers are less important
            public StyleEntry(UIStyleGroup sourceGroup, UIStyle style, StyleType type, StyleState state, int styleNumber, int attributeCount) {
                this.sourceGroup = sourceGroup;
                this.style = style;
                this.type = type;
                this.state = state;
                this.priority = (int)BitUtil.SetBytes(styleNumber, attributeCount, (int) type, (int) state);
            }

        }

    }

}