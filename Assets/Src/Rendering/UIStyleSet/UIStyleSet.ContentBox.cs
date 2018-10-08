using JetBrains.Annotations;
using Src;

namespace Rendering {

    public partial class UIStyleSet {

#region Margin

        [PublicAPI]
        public UIMeasurement GetMarginTop(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.MarginTop, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetMarginRight(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.MarginRight, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetMarginBottom(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.MarginBottom, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetMarginLeft(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.MarginLeft, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public void SetMargin(ContentBoxRect value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.MarginTop = value.top;
            style.MarginRight = value.right;
            style.MarginBottom = value.bottom;
            style.MarginLeft = value.left;

            if ((state & currentState) != 0) {
                return;
            }

            if (style == GetActiveStyleForProperty(StylePropertyId.MarginTop)) {
                computedStyle.MarginTop = value.top;
            }

            if (style == GetActiveStyleForProperty(StylePropertyId.MarginRight)) {
                computedStyle.MarginRight = value.right;
            }

            if (style == GetActiveStyleForProperty(StylePropertyId.MarginBottom)) {
                computedStyle.MarginBottom = value.bottom;
            }

            if (style == GetActiveStyleForProperty(StylePropertyId.MarginLeft)) {
                computedStyle.MarginLeft = value.left;
            }

        }

        [PublicAPI]
        public ContentBoxRect GetMargin(StyleState state) {
            return new ContentBoxRect(
                GetMarginTop(state),
                GetMarginRight(state),
                GetMarginBottom(state),
                GetMarginLeft(state)
            );
        }

        [PublicAPI]
        public void SetMarginTop(UIMeasurement value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.MarginTop = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.MarginTop)) {
                computedStyle.MarginTop = value;
            }
        }

        [PublicAPI]
        public void SetMarginRight(UIMeasurement value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.MarginRight = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.MarginRight)) {
                computedStyle.MarginRight = value;
            }
        }

        [PublicAPI]
        public void SetMarginBottom(UIMeasurement value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.MarginBottom = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.MarginBottom)) {
                computedStyle.MarginBottom = value;
            }
        }

        [PublicAPI]
        public void SetMarginLeft(UIMeasurement value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.MarginLeft = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.MarginLeft)) {
                computedStyle.MarginLeft = value;
            }
        }

#endregion

#region Padding

        [PublicAPI]
        public UIMeasurement GetPaddingTop(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.PaddingTop, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetPaddingRight(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.PaddingRight, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetPaddingBottom(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.PaddingBottom, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public UIMeasurement GetPaddingLeft(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.PaddingLeft, state);
            return property.IsDefined
                ? UIMeasurement.Decode(property.valuePart0, property.valuePart1)
                : UIMeasurement.Unset;
        }

        [PublicAPI]
        public void SetPadding(PaddingBox value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.PaddingTop = value.top;
            style.PaddingRight = value.right;
            style.PaddingBottom = value.bottom;
            style.PaddingLeft = value.left;

            if ((state & currentState) != 0) {
                return;
            }

            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.PaddingTop)) {
                computedStyle.PaddingTop = value.top;
            }

            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.PaddingRight)) {
                computedStyle.PaddingRight = value.right;
            }

            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.PaddingBottom)) {
                computedStyle.PaddingBottom = value.bottom;
            }

            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.PaddingLeft)) {
                computedStyle.PaddingLeft = value.left;
            }

        }

        [PublicAPI]
        public ContentBoxRect GetPadding(StyleState state) {
            return new ContentBoxRect(
                GetPaddingTop(state),
                GetPaddingRight(state),
                GetPaddingBottom(state),
                GetPaddingLeft(state)
            );
        }

        [PublicAPI]
        public void SetPaddingTop(UIFixedLength value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.PaddingTop = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.PaddingTop)) {
                computedStyle.PaddingTop = value;
            }
        }

        [PublicAPI]
        public void SetPaddingRight(UIFixedLength value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.PaddingRight = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.PaddingRight)) {
                computedStyle.PaddingRight = value;
            }
        }

        [PublicAPI]
        public void SetPaddingBottom(UIFixedLength value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.PaddingBottom = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.PaddingBottom)) {
                computedStyle.PaddingBottom = value;
            }
        }

        [PublicAPI]
        public void SetPaddingLeft(UIFixedLength value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.PaddingLeft = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.PaddingLeft)) {
                computedStyle.PaddingLeft = value;
            }
        }

#endregion

#region Border

        [PublicAPI]
        public UIFixedLength GetBorderTop(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.BorderTop, state);
            return property.IsDefined
                ? UIFixedLength.Decode(property.valuePart0, property.valuePart1)
                : UIFixedLength.Unset;
        }

        [PublicAPI]
        public UIFixedLength GetBorderRight(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.BorderRight, state);
            return property.IsDefined
                ? UIFixedLength.Decode(property.valuePart0, property.valuePart1)
                : UIFixedLength.Unset;
        }

        [PublicAPI]
        public UIFixedLength GetBorderBottom(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.BorderBottom, state);
            return property.IsDefined
                ? UIFixedLength.Decode(property.valuePart0, property.valuePart1)
                : UIFixedLength.Unset;
        }

        [PublicAPI]
        public UIFixedLength GetBorderLeft(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.BorderLeft, state);
            return property.IsDefined
                ? UIFixedLength.Decode(property.valuePart0, property.valuePart1)
                : UIFixedLength.Unset;
        }

        [PublicAPI]
        public void SetBorder(PaddingBox value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderTop = value.top;
            style.BorderRight = value.right;
            style.BorderBottom = value.bottom;
            style.BorderLeft = value.left;

            if ((state & currentState) != 0) {
                return;
            }

            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderTop)) {
                computedStyle.BorderTop = value.top;
            }

            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderRight)) {
                computedStyle.BorderRight = value.right;
            }

            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderBottom)) {
                computedStyle.BorderBottom = value.bottom;
            }

            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderLeft)) {
                computedStyle.BorderLeft = value.left;
            }

        }

        [PublicAPI]
        public PaddingBox GetBorder(StyleState state) {
            return new PaddingBox(
                GetBorderTop(state),
                GetBorderRight(state),
                GetBorderBottom(state),
                GetBorderLeft(state)
            );
        }

        [PublicAPI]
        public void SetBorderTop(UIFixedLength value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderTop = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderTop)) {
                computedStyle.BorderTop = value;
            }
        }

        [PublicAPI]
        public void SetBorderRight(UIFixedLength value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderRight = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderRight)) {
                computedStyle.BorderRight = value;
            }
        }

        [PublicAPI]
        public void SetBorderBottom(UIFixedLength value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderBottom = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderBottom)) {
                computedStyle.BorderBottom = value;
            }
        }

        [PublicAPI]
        public void SetBorderLeft(UIFixedLength value, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.BorderLeft = value;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.BorderLeft)) {
                computedStyle.BorderLeft = value;
            }
        }

#endregion

    }

}