namespace UIForia.Rendering {

    public partial class UIStyleSet {

        public void SetGridItemPlacement(int colStart, int colSpan, int rowStart, int rowSpan, StyleState state) {
            SetGridItemColStart(colStart, state);
            SetGridItemColSpan(colSpan, state);
            SetGridItemRowStart(rowStart, state);
            SetGridItemRowSpan(rowSpan, state);
        }

        public int GetGridItemColStart(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemColStart, state).AsInt;
        }

        public int GetGridItemColSpan(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemColSpan, state).AsInt;
        }

        public int GetGridItemRowStart(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemRowStart, state).AsInt;
        }

        public int GetGridItemRowSpan(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridItemRowSpan, state).AsInt;
        }

        public void SetGridItemColStart(int colStart, StyleState state) {
            SetIntProperty(StylePropertyId.GridItemColStart, colStart, state);
        }

        public void SetGridItemColSpan(int colSpan, StyleState state) {
            SetIntProperty(StylePropertyId.GridItemColSpan, colSpan, state);
        }

        public void SetGridItemRowStart(int rowStart, StyleState state) {
            SetIntProperty(StylePropertyId.GridItemRowStart, rowStart, state);
        }

        public void SetGridItemRowSpan(int rowSpan, StyleState state) {
            SetIntProperty(StylePropertyId.GridItemRowSpan, rowSpan, state);
        }

    }

}