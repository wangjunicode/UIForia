using Src.Layout;

namespace Rendering {

    public partial class UIStyleSet {

        public LayoutWrap GetFlexWrapMode(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.FlexWrap, state);
            return property.IsDefined ? (LayoutWrap) property.valuePart0 : LayoutWrap.Unset;
        }

        public LayoutDirection GetFlexLayoutDirection(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.FlexDirection, state);
            return property.IsDefined ? (LayoutDirection) property.valuePart0 : LayoutDirection.Unset;
        }

        public MainAxisAlignment GetFlexLayoutMainAlignment(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.FlexMainAxisAlignment, state);
            return property.IsDefined ? (MainAxisAlignment) property.valuePart0 : MainAxisAlignment.Unset;
        }

        public CrossAxisAlignment GetFlexLayoutCrossAlignment(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.FlexCrossAxisAlignment, state);
            return property.IsDefined ? (CrossAxisAlignment) property.valuePart0 : CrossAxisAlignment.Unset;
        }

        public void SetFlexWrapMode(LayoutWrap wrapMode, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexLayoutWrap = wrapMode;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexWrap)) {
                computedStyle.FlexLayoutWrap = wrapMode;
            }
        }

        public void SetFlexDirection(LayoutDirection direction, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexLayoutDirection = direction;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexDirection)) {
                computedStyle.FlexLayoutDirection = direction;
            }
        }

        public void SetFlexMainAxisAlignment(MainAxisAlignment alignment, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexLayoutMainAxisAlignment = alignment;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexMainAxisAlignment)) {
                computedStyle.FlexLayoutMainAxisAlignment = alignment;
            }
        }

        public void SetFlexCrossAxisAlignment(CrossAxisAlignment alignment, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexLayoutCrossAxisAlignment = alignment;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexCrossAxisAlignment)) {
                computedStyle.FlexLayoutCrossAxisAlignment = alignment;
            }
        }

    }

}