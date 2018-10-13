namespace Rendering {

    public partial class UIStyleSet {

        public void SetGridItemPlacement(int colStart, int colSpan, int rowStart, int rowSpan, StyleState state) {
            SetGridItemColStart(colStart, state);
            SetGridItemColSpan(colSpan, state);
            SetGridItemRowStart(rowStart, state);
            SetGridItemRowSpan(rowSpan, state);
        }

        public int GetGridItemColStart(StyleState state) {
            return GetIntValue(StylePropertyId.GridItemColStart, state);
        }

        public int GetGridItemColSpan(StyleState state) {
            return GetIntValue(StylePropertyId.GridItemColSpan, state);
        }

        public int GetGridItemRowStart(StyleState state) {
            return GetIntValue(StylePropertyId.GridItemRowStart, state);
        }

        public int GetGridItemRowSpan(StyleState state) {
            return GetIntValue(StylePropertyId.GridItemRowSpan, state);
        }

        public void SetGridItemColStart(int colStart, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.GridItemColStart = colStart;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.GridItemColStart)) {
                computedStyle.GridItemColStart = colStart;
            }
        }

        public void SetGridItemColSpan(int colSpan, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.GridItemColSpan = colSpan;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.GridItemColSpan)) {
                computedStyle.GridItemColSpan = colSpan;
            }
        }

        public void SetGridItemRowStart(int rowStart, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.GridItemRowStart = rowStart;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.GridItemRowStart)) {
                computedStyle.GridItemRowStart = rowStart;
            }
        }

        public void SetGridItemRowSpan(int rowSpan, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.GridItemRowSpan = rowSpan;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.GridItemRowSpan)) {
                computedStyle.GridItemRowSpan = rowSpan;
            }
        }

    }

}