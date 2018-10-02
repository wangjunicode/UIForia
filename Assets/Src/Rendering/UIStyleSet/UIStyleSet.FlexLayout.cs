using Src.Layout;

namespace Rendering {

    public partial class UIStyleSet {

        public LayoutWrap GetFlexWrapMode(StyleState state) {
            return (LayoutWrap) GetEnumValue(StylePropertyId.FlexWrap, state);
        }

        public LayoutDirection GetFlexLayoutDirection(StyleState state) {
            return (LayoutDirection) GetEnumValue(StylePropertyId.FlexDirection, state);
        }

        public MainAxisAlignment GetFlexLayoutMainAlignment(StyleState state) {
            return (MainAxisAlignment) GetEnumValue(StylePropertyId.FlexMainAxisAlignment, state);
        }

        public CrossAxisAlignment GetFlexLayoutCrossAlignment(StyleState state) {
            return (CrossAxisAlignment) GetEnumValue(StylePropertyId.FlexCrossAxisAlignment, state);
        }

        public void SetFlexWrapMode(LayoutWrap wrapMode, StyleState state) {
            SetEnumProperty(StylePropertyId.FlexWrap, (int) wrapMode, state);
        }

        public void SetFlexDirection(LayoutDirection direction, StyleState state) {
            SetEnumProperty(StylePropertyId.FlexDirection, (int) direction, state);
        }

        public void SetFlexMainAxisAlignment(MainAxisAlignment alignment, StyleState state) {
            SetEnumProperty(StylePropertyId.FlexMainAxisAlignment, (int) alignment, state);
        }

        public void SetFlexCrossAxisAlignment(CrossAxisAlignment alignment, StyleState state) {
            SetEnumProperty(StylePropertyId.FlexCrossAxisAlignment, (int) alignment, state);
        }

    }

}