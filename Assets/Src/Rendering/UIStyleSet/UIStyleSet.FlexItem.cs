using Src.Layout;

namespace Rendering {

    public partial class UIStyleSet {

        public int GetFlexItemGrowthFactor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexItemGrow, state).AsInt;
        }

        public int GetFlexItemShrinkFactor(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexItemShrink, state).AsInt;
        }

        public int GetFlexItemOrderOverride(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexItemOrder, state).AsInt;
        }

        public CrossAxisAlignment GetFlexItemSelfAlignment(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.FlexItemSelfAlignment, state).AsCrossAxisAlignment;
        }

        public void SetFlexItemGrowFactor(int growthFactor, StyleState state) {
            SetIntProperty(StylePropertyId.FlexItemGrow, growthFactor, state);
        }

        public void SetFlexItemShrinkFactor(int shrinkFactor, StyleState state) {
            SetIntProperty(StylePropertyId.FlexItemShrink, shrinkFactor, state);
        }

        public void SetFlexItemOrderOverride(int order, StyleState state) {
            SetIntProperty(StylePropertyId.FlexItemOrder, order, state);
        }

        public void SetFlexItemSelfAlignment(CrossAxisAlignment alignment, StyleState state) {
            SetEnumProperty(StylePropertyId.FlexItemSelfAlignment, (int) alignment, state);
        }

    }

}