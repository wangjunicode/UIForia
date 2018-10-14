using Src.Layout;

namespace Rendering {

    public partial class UIStyleSet {

        public int GetFlexItemGrowthFactor(StyleState state) {
            return GetIntValue(StylePropertyId.FlexItemGrow, state);
        }

        public int GetFlexItemShrinkFactor(StyleState state) {
            return GetIntValue(StylePropertyId.FlexItemShrink, state);
        }

        public int GetFlexItemOrderOverride(StyleState state) {
            return GetIntValue(StylePropertyId.FlexItemOrder, state);
        }

        public CrossAxisAlignment GetFlexItemSelfAlignment(StyleState state) {
            return (CrossAxisAlignment)GetEnumProperty(StylePropertyId.FlexItemSelfAlignment, state);
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