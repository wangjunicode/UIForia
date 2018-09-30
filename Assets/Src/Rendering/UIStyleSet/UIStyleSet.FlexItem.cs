using Src.Layout;

namespace Rendering {

    public partial class UIStyleSet {

        public int GetFlexItemGrowthFactor(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.FlexItemGrow, state);
            return property.IsDefined
                ? property.valuePart0
                : IntUtil.UnsetValue;
        }

        public int GetFlexItemShrinkFactor(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.FlexItemShrink, state);
            return property.IsDefined
                ? property.valuePart0
                : IntUtil.UnsetValue;
        }

        public int GetFlexItemOrderOverride(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.FlexItemOrder, state);
            return property.IsDefined
                ? property.valuePart0
                : IntUtil.UnsetValue;
        }

        public CrossAxisAlignment GetFlexItemSelfAlignment(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.FlexItemSelfAlignment, state);
            return property.IsDefined
                ? (CrossAxisAlignment) property.valuePart0
                : CrossAxisAlignment.Unset;
        }

        public void SetFlexItemGrowFactor(int growthFactor, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexItemGrowthFactor = growthFactor;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexItemGrow)) {
                int oldFactor = computedStyle.flexGrowthFactor;
                if (oldFactor != growthFactor) {
                    computedStyle.flexGrowthFactor = growthFactor;
                    styleSystem.SetGrowthFactor(element, growthFactor, oldFactor);
                }
            }
        }
        
        public void SetFlexItemShrinkFactor(int shrinkFactor, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexItemShrinkFactor = shrinkFactor;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexItemShrink)) {
                int oldFactor = computedStyle.flexShrinkFactor;
                if (oldFactor != shrinkFactor) {
                    computedStyle.flexShrinkFactor = shrinkFactor;
                    styleSystem.SetShrinkFactor(element, shrinkFactor, oldFactor);
                }
            }
        }

        public void SetFlexItemOrderOverride(int order, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexItemOrder = order;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexItemOrder)) {
                int oldOrder = computedStyle.flexOrderOverride;
                if (oldOrder != order) {
                    computedStyle.flexOrderOverride = order;
                    styleSystem.SetFlexOrderOverride(element, order, oldOrder);
                }
            }
        }
        
        public void SetFlexItemSelfAlignment(CrossAxisAlignment alignment, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.FlexItemSelfAlign = alignment;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.FlexItemSelfAlignment)) {
                CrossAxisAlignment oldAlignment = computedStyle.flexSelfAlignment;
                if (oldAlignment != alignment) {
                    computedStyle.flexSelfAlignment = alignment;
                    styleSystem.SetFlexSelfAlignment(element, alignment, oldAlignment);
                }
            }
        }
        
    }

}