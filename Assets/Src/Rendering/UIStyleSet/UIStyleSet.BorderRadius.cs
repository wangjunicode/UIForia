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
            SetBorderRadiusBottomLeft(newBorderRadius.bottomLeft, state);
            SetBorderRadiusBottomRight(newBorderRadius.bottomRight, state);
            SetBorderRadiusTopRight(newBorderRadius.topRight, state);
            SetBorderRadiusTopLeft(newBorderRadius.topLeft, state);
        }

        [PublicAPI]
        public UIMeasurement GetBorderRadiusTopLeft(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.BorderRadiusTopLeft, state);
        }

        [PublicAPI]
        public UIMeasurement GetBorderRadiusTopRight(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.BorderRadiusTopRight, state);
        }

        [PublicAPI]
        public UIMeasurement GetBorderRadiusBottomLeft(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.BorderRadiusBottomLeft, state);
        }

        [PublicAPI]
        public UIMeasurement GetBorderRadiusBottomRight(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.BorderRadiusBottomRight, state);
        }

        [PublicAPI]
        public void SetBorderRadiusTopLeft(UIMeasurement value, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.BorderRadiusTopLeft, value, state);
        }

        [PublicAPI]
        public void SetBorderRadiusTopRight(UIMeasurement value, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.BorderRadiusTopRight, value, state);
        }

        [PublicAPI]
        public void SetBorderRadiusBottomRight(UIMeasurement value, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.BorderRadiusBottomRight, value, state);
        }

        [PublicAPI]
        public void SetBorderRadiusBottomLeft(UIMeasurement value, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.BorderRadiusBottomLeft, value, state);
        }

    }

}