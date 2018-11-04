using JetBrains.Annotations;
using Src;

namespace Src.Rendering {

    public partial class UIStyleSet {

#region Margin

        [PublicAPI]
        public UIMeasurement GetMarginTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginTop, state).AsUIMeasurement;
        }

        [PublicAPI]
        public UIMeasurement GetMarginRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginRight, state).AsUIMeasurement;
        }

        [PublicAPI]
        public UIMeasurement GetMarginBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginBottom, state).AsUIMeasurement;
        }

        [PublicAPI]
        public UIMeasurement GetMarginLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MarginLeft, state).AsUIMeasurement;
        }

        [PublicAPI]
        public void SetMargin(ContentBoxRect value, StyleState state) {
            
            SetMarginTop(value.top, state);
            SetMarginRight(value.right, state);
            SetMarginBottom(value.bottom, state);
            SetMarginLeft(value.bottom, state);
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
            SetUIMeasurementProperty(StylePropertyId.MarginTop, value, state);
        }

        [PublicAPI]
        public void SetMarginRight(UIMeasurement value, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.MarginRight, value, state);
        }

        [PublicAPI]
        public void SetMarginBottom(UIMeasurement value, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.MarginBottom, value, state);
        }

        [PublicAPI]
        public void SetMarginLeft(UIMeasurement value, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.MarginLeft, value, state);
        }

#endregion

#region Padding

        [PublicAPI]
        public UIFixedLength GetPaddingTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingTop, state).AsUIFixedLength;
        }

        [PublicAPI]
        public UIFixedLength GetPaddingRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingRight, state).AsUIFixedLength;
        }

        [PublicAPI]
        public UIFixedLength GetPaddingBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingBottom, state).AsUIFixedLength;
        }

        [PublicAPI]
        public UIFixedLength GetPaddingLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PaddingLeft, state).AsUIFixedLength;
        }

        [PublicAPI]
        public void SetPadding(FixedLengthRect value, StyleState state) {
            SetPaddingTop(value.top, state);
            SetPaddingRight(value.right, state);
            SetPaddingBottom(value.bottom, state);
            SetPaddingLeft(value.left, state);
        }

        [PublicAPI]
        public FixedLengthRect GetPadding(StyleState state) {
            return new FixedLengthRect(
                GetPaddingTop(state),
                GetPaddingRight(state),
                GetPaddingBottom(state),
                GetPaddingLeft(state)
            );
        }

        [PublicAPI]
        public void SetPaddingTop(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.PaddingTop, value, state);
        }

        [PublicAPI]
        public void SetPaddingRight(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.PaddingRight, value, state);
        }

        [PublicAPI]
        public void SetPaddingBottom(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.PaddingBottom, value, state);
        }

        [PublicAPI]
        public void SetPaddingLeft(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.PaddingLeft, value, state);
        }

#endregion

#region Border

        [PublicAPI]
        public UIFixedLength GetBorderTop(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderTop, state).AsUIFixedLength;
        }

        [PublicAPI]
        public UIFixedLength GetBorderRight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderRight, state).AsUIFixedLength;
        }

        [PublicAPI]
        public UIFixedLength GetBorderBottom(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderBottom, state).AsUIFixedLength;
        }

        [PublicAPI]
        public UIFixedLength GetBorderLeft(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.BorderLeft, state).AsUIFixedLength;
        }

        [PublicAPI]
        public void SetBorder(FixedLengthRect value, StyleState state) {
            SetBorderTop(value.top, state);
            SetBorderRight(value.right, state);
            SetBorderBottom(value.bottom, state);
            SetBorderLeft(value.left, state);
        }

        [PublicAPI]
        public FixedLengthRect GetBorder(StyleState state) {
            return new FixedLengthRect(
                GetBorderTop(state),
                GetBorderRight(state),
                GetBorderBottom(state),
                GetBorderLeft(state)
            );
        }

        [PublicAPI]
        public void SetBorderTop(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.BorderTop, value, state);
        }

        [PublicAPI]
        public void SetBorderRight(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.BorderRight, value, state);
        }

        [PublicAPI]
        public void SetBorderBottom(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.BorderBottom, value, state);
        }

        [PublicAPI]
        public void SetBorderLeft(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.BorderLeft, value, state);
        }

#endregion

    }

}