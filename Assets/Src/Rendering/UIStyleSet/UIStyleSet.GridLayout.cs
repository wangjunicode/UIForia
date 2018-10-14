using Src.Layout;

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

    }

}