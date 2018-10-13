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
            return (CrossAxisAlignment) GetEnumValue(StylePropertyId.GridLayoutColAlignment, state);
        }

        public CrossAxisAlignment GetGridLayoutRowAlignment(StyleState state) {
            return (CrossAxisAlignment) GetEnumValue(StylePropertyId.GridLayoutRowAlignment, state);
        }

    }

}