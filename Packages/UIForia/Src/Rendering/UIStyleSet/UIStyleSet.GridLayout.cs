using System.Collections.Generic;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;

namespace UIForia.Rendering {

    public partial class UIStyleSet {

        public void SetGridLayoutColAlignment(CrossAxisAlignment alignment, StyleState state) {
            SetEnumProperty(StylePropertyId.GridLayoutColAlignment, (int) alignment, state);
        }

        public void SetGridLayoutRowAlignment(CrossAxisAlignment alignment, StyleState state) {
            SetEnumProperty(StylePropertyId.GridLayoutRowAlignment, (int) alignment, state);
        }

        public CrossAxisAlignment GetGridLayoutColAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColAlignment, state).AsCrossAxisAlignment;
        }

        public CrossAxisAlignment GetGridLayoutRowAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowAlignment, state).AsCrossAxisAlignment;
        }

        public GridLayoutDensity GetGridLayoutDensity(StyleState state) {
            return (GridLayoutDensity)GetPropertyValueInState(StylePropertyId.GridLayoutDensity, state).AsInt;
        }

        public void SetGridLayoutDensity(GridLayoutDensity density, StyleState state) {
            SetEnumProperty(StylePropertyId.GridLayoutDensity, (int) density, state);
        }

        public void SetGridLayoutColGap(float colGap, StyleState state) {
            SetFloatProperty(StylePropertyId.GridLayoutColGap, colGap, state);
        }

        public void SetGridLayoutRowGap(float rowGap, StyleState state) {
            SetFloatProperty(StylePropertyId.GridLayoutRowGap, rowGap, state);
        }

        public void SetGridLayoutColAutoSize(GridTrackSize size, StyleState state) {
            SetGridTrackSizeProperty(StylePropertyId.GridLayoutColAutoSize, size, state);
        }

        public void SetGridLayoutRowAutoSize(GridTrackSize size, StyleState state) {
            SetGridTrackSizeProperty(StylePropertyId.GridLayoutRowAutoSize, size, state);
        }

        public void SetGridLayoutColTemplate(IReadOnlyList<GridTrackSize> colTemplate, StyleState state) {
            SetObjectProperty(StylePropertyId.GridLayoutColTemplate, colTemplate, state);
        }

        public void SetGridLayoutRowTemplate(IReadOnlyList<GridTrackSize> rowTemplate, StyleState state) {
            SetObjectProperty(StylePropertyId.GridLayoutRowTemplate, rowTemplate, state);
        }

        public float GetGridLayoutColGap(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutColGap, state).AsFloat;
        }

        public float GetGridLayoutRowGap(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.GridLayoutRowGap, state).AsFloat;
        }

    }

}