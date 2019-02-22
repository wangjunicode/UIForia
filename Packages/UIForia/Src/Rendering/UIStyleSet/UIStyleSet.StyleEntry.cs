
namespace UIForia.Rendering {

    public partial class UIStyleSet {

        private struct StyleEntry {

            public readonly UIStyle style;
            public readonly StyleState state;
            public readonly StyleType type;
            public readonly int priority;

            //style number is used to prioritize shared styles, higher numbers are less important
            public StyleEntry(UIStyle style, StyleType type, StyleState state, int styleNumber, int attributeCount) {
                this.style = style;
                this.type = type;
                this.state = state;
                this.priority = GetSortPriority(type, state, styleNumber, attributeCount);
            }

            private static int GetSortPriority(StyleType type, StyleState state, int styleNumber, int attributeCount) {
                return BitUtil.SetBytes(styleNumber, attributeCount, (int) type, (int) state);
            }
        }
    }
}