using JetBrains.Annotations;
using Src;
using Src.Systems;

namespace Rendering {

    [PublicAPI]
    public partial class UIStyleSet {

        [PublicAPI]
        public UIMeasurement GetMinWidth(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.MinWidth, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetMaxWidth(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.MaxWidth, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetPreferredWidth(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.PreferredWidth, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public void SetMinWidth(UIMeasurement measurement, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.MinWidth = measurement;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.MinWidth)) {
                computedStyle.MinWidth = measurement;
            }
        }

        [PublicAPI]
        public void SetMaxWidth(UIMeasurement measurement, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.MaxWidth = measurement;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.MaxWidth)) {
                computedStyle.MaxWidth = measurement;
            }
        }

        [PublicAPI]
        public void SetPreferredWidth(UIMeasurement measurement, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.PreferredWidth = measurement;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.PreferredWidth)) {
                computedStyle.PreferredWidth = measurement;
            }
        }

        [PublicAPI]
        public UIMeasurement GetMinHeight(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.MinHeight, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetMaxHeight(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.MaxHeight, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetPreferredHeight(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.PreferredHeight, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public void SetMinHeight(UIMeasurement measurement, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.MinHeight = measurement;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.MinHeight)) {
                computedStyle.MinHeight = measurement;
            }
        }

        [PublicAPI]
        public void SetMaxHeight(UIMeasurement measurement, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.MaxHeight = measurement;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.MaxHeight)) {
                computedStyle.MaxHeight = measurement;
            }
        }

        [PublicAPI]
        public void SetPreferredHeight(UIMeasurement measurement, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.PreferredHeight = measurement;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.PreferredHeight)) {
                computedStyle.PreferredHeight = measurement;
            }
        }

    }

}