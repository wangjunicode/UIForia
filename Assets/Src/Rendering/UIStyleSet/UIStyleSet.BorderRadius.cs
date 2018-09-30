using JetBrains.Annotations;
using Src;

namespace Rendering {

    public partial class UIStyleSet {

        [PublicAPI]
        public BorderRadius GetBorderRadius(StyleState state) {
            return new BorderRadius(
                GetBorderRadiusTopLeft(state),
                GetBorderRadiusTopRight(state),
                GetBorderRadiusBottomRight(state),
                GetBorderRadiusBottomLeft(state)
            );
        }

        [PublicAPI]
        public void SetBorderRadius(BorderRadius newBorderRadius, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderRadiusTopLeft = newBorderRadius.topLeft;
            style.BorderRadiusTopRight = newBorderRadius.topRight;
            style.BorderRadiusBottomRight = newBorderRadius.bottomRight;
            style.BorderRadiusBottomLeft = newBorderRadius.bottomLeft;

            bool updated = false;

            if (style == GetActiveStyleForProperty(StylePropertyId.BorderRadiusTopLeft)) {
                computedStyle.RareData.borderRadiusTopLeft = newBorderRadius.topLeft;
                updated = true;
            }

            if (style == GetActiveStyleForProperty(StylePropertyId.BorderRadiusTopRight)) {
                computedStyle.RareData.borderRadiusTopRight = newBorderRadius.topRight;
                updated = true;
            }

            if (style == GetActiveStyleForProperty(StylePropertyId.BorderRadiusBottomRight)) {
                computedStyle.RareData.borderRadiusBottomRight = newBorderRadius.bottomRight;
                updated = true;
            }

            if (style == GetActiveStyleForProperty(StylePropertyId.BorderRadiusBottomLeft)) {
                computedStyle.RareData.borderRadiusBottomLeft = newBorderRadius.bottomLeft;
                updated = true;
            }

            if (updated) {
                styleSystem.SetBorderRadius(element, computedStyle.RareData.borderRadius);
            }
        }

        [PublicAPI]
        public UIMeasurement GetBorderRadiusTopLeft(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.BorderRadiusTopLeft, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetBorderRadiusTopRight(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.BorderRadiusTopRight, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetBorderRadiusBottomLeft(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.BorderRadiusBottomLeft, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetBorderRadiusBottomRight(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.BorderRadiusBottomRight, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public void SetBorderRadiusTopLeft(UIMeasurement value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderRadiusTopLeft = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderRadiusTopLeft)) {
                computedStyle.RareData.borderRadiusTopLeft = value;
                styleSystem.SetBorderRadius(element, computedStyle.RareData.borderRadius);
            }
        }

        [PublicAPI]
        public void SetBorderRadiusTopRight(UIMeasurement value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderRadiusTopRight = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderRadiusTopRight)) {
                computedStyle.RareData.borderRadiusTopRight = value;
                styleSystem.SetBorderRadius(element, computedStyle.RareData.borderRadius);
            }
        }

        [PublicAPI]
        public void SetBorderRadiusBottomRight(UIMeasurement value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderRadiusBottomRight = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderRadiusBottomRight)) {
                computedStyle.RareData.borderRadiusBottomRight = value;
                styleSystem.SetBorderRadius(element, computedStyle.RareData.borderRadius);
            }
        }

        [PublicAPI]
        public void SetBorderRadiusBottomLeft(UIMeasurement value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderRadiusBottomLeft = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderRadiusBottomLeft)) {
                computedStyle.RareData.borderRadiusBottomLeft = value;
                styleSystem.SetBorderRadius(element, computedStyle.RareData.borderRadius);
            }
        }

    }

}