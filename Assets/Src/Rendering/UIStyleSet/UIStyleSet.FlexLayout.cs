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
                LayoutWrap oldWrapMode = computedStyle.flexWrapMode;
                if (oldWrapMode != wrapMode) {
                    computedStyle.flexWrapMode = wrapMode;
                    styleSystem.SetLayoutWrap(element, wrapMode, oldWrapMode);
                }
            }
        }
        
        public void SetFlexDirection(LayoutDirection direction, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexLayoutDirection = direction;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexDirection)) {
                if (computedStyle.flexDirection != direction) {
                    computedStyle.flexDirection = direction;
                    styleSystem.SetLayoutDirection(element, direction);
                }
            }
        }

        public void SetFlexMainAxisAlignment(MainAxisAlignment alignment, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexLayoutMainAxisAlignment = alignment;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexMainAxisAlignment)) {
                MainAxisAlignment oldAlignment = computedStyle.flexMainAxisAlignment;
                if (computedStyle.flexMainAxisAlignment != alignment) {
                    computedStyle.flexMainAxisAlignment = alignment;
                    styleSystem.SetMainAxisAlignment(element, alignment, oldAlignment);
                }
            }
        }

        public void SetFlexCrossAxisAlignment(CrossAxisAlignment alignment, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexLayoutCrossAxisAlignment = alignment;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexCrossAxisAlignment)) {
                CrossAxisAlignment oldAlignment = computedStyle.flexCrossAxisAlignment;
                if (computedStyle.flexCrossAxisAlignment != alignment) {
                    computedStyle.flexCrossAxisAlignment = alignment;
                    styleSystem.SetCrossAxisAlignment(element, alignment, oldAlignment);
                }
            }
        }
    }

}