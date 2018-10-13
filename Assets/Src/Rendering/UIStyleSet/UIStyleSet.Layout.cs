using System.Collections.Generic;
using Src.Layout;
using Src.Layout.LayoutTypes;

namespace Rendering {

    public partial class UIStyleSet {

        public LayoutType GetLayoutType(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.LayoutType, state);
            return property.IsDefined ? (LayoutType) property.valuePart0 : LayoutType.Unset;
        }

        public void SetLayoutType(LayoutType layoutType, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.LayoutType = layoutType;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.LayoutType)) {
                computedStyle.LayoutType = layoutType;
            }
        }

        public LayoutBehavior GetLayoutBehavior(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.LayoutBehavior, state);
            return property.IsDefined ? (LayoutBehavior) property.valuePart0 : LayoutBehavior.Unset;
        }

        public void SetLayoutBehavior(LayoutBehavior behavior, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.LayoutBehavior = behavior;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.LayoutBehavior)) {
                computedStyle.LayoutBehavior = behavior;
            }
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