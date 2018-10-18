using System.Collections.Generic;
using Src.Layout;
using Src.Layout.LayoutTypes;

namespace Rendering {

    public partial class UIStyleSet {

        public void SetGridLayoutColAlignment(CrossAxisAlignment alignment, StyleState state) {
            SetEnumProperty(StylePropertyId.GridLayoutColAlignment, (int) alignment, state);
        }

        public void SetGridLayoutRowAlignment(CrossAxisAlignment alignment, StyleState state) {
            SetEnumProperty(StylePropertyId.GridLayoutRowAlignment, (int) alignment, state);
        }

        public CrossAxisAlignment GetGridLayoutColAlignment(StyleState state) {
            return (CrossAxisAlignment) GetEnumProperty(StylePropertyId.GridLayoutColAlignment, state);
        }

        public CrossAxisAlignment GetGridLayoutRowAlignment(StyleState state) {
            return (CrossAxisAlignment) GetEnumProperty(StylePropertyId.GridLayoutRowAlignment, state);
        }

        public GridLayoutDensity GetGridLayoutDensity(StyleState state) {
            return (GridLayoutDensity) GetEnumProperty(StylePropertyId.GridLayoutDensity, state);
        }

        public void SetGridLayoutDensity(GridLayoutDensity density, StyleState state) {
            SetEnumProperty(StylePropertyId.GridLayoutDensity, (int) density, state);
        }

        public void SetGridLayoutColGap(float colGap, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.GridLayoutColGapSize = colGap;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.GridLayoutColGap)) {
                computedStyle.GridLayoutColGap = colGap;
            }
        }

        public void SetGridLayoutRowGap(float rowGap, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.GridLayoutRowGapSize = rowGap;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.GridLayoutRowGap)) {
                computedStyle.GridLayoutRowGap = rowGap;
            }
        }

        public void SetGridLayoutColAutoSize(GridTrackSize size, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.GridLayoutColAutoSize = size;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.GridLayoutColAutoSize)) {
                computedStyle.GridLayoutColAutoSize = size;
            }
        }

        public void SetGridLayoutRowAutoSize(GridTrackSize size, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.GridLayoutRowAutoSize = size;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.GridLayoutRowAutoSize)) {
                computedStyle.GridLayoutRowAutoSize = size;
            }
        }

        public void SetGridLayoutColTemplate(IReadOnlyList<GridTrackSize> colTemplate, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.GridLayoutColTemplate = colTemplate;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.GridLayoutColTemplate)) {
                computedStyle.GridLayoutColTemplate = colTemplate;
            }
        }

        public void SetGridLayoutRowTemplate(IReadOnlyList<GridTrackSize> rowTemplate, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.GridLayoutRowTemplate = rowTemplate;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.GridLayoutRowTemplate)) {
                computedStyle.GridLayoutRowTemplate = rowTemplate;
            }
        }

        public float GetGridLayoutColGap(StyleState state) {
            return GetFloatValue(StylePropertyId.GridLayoutColGap, state);
        }

        public float GetGridLayoutRowGap(StyleState state) {
            return GetFloatValue(StylePropertyId.GridLayoutRowGap, state);
        }

    }

}