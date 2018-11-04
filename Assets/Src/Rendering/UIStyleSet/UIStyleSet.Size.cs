using JetBrains.Annotations;
using Src;

namespace Src.Rendering {

    [PublicAPI]
    public partial class UIStyleSet {

        [PublicAPI]
        public UIMeasurement GetMinWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MinWidth, state).AsUIMeasurement;
        }

        [PublicAPI]
        public UIMeasurement GetMaxWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MaxWidth, state).AsUIMeasurement;
        }

        [PublicAPI]
        public UIMeasurement GetPreferredWidth(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PreferredWidth, state).AsUIMeasurement;
        }

        [PublicAPI]
        public void SetMinWidth(UIMeasurement measurement, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.MinWidth, measurement, state);
        }

        [PublicAPI]
        public void SetMaxWidth(UIMeasurement measurement, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.MaxWidth, measurement, state);
        }

        [PublicAPI]
        public void SetPreferredWidth(UIMeasurement measurement, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.PreferredWidth, measurement, state);
        }

        [PublicAPI]
        public UIMeasurement GetMinHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MinHeight, state).AsUIMeasurement;
        }

        [PublicAPI]
        public UIMeasurement GetMaxHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.MaxHeight, state).AsUIMeasurement;
        }

        [PublicAPI]
        public UIMeasurement GetPreferredHeight(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.PreferredHeight, state).AsUIMeasurement;
        }

        [PublicAPI]
        public void SetMinHeight(UIMeasurement measurement, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.MinHeight, measurement, state);
        }

        [PublicAPI]
        public void SetMaxHeight(UIMeasurement measurement, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.MaxHeight, measurement, state);
        }

        [PublicAPI]
        public void SetPreferredHeight(UIMeasurement measurement, StyleState state) {
            SetUIMeasurementProperty(StylePropertyId.PreferredHeight, measurement, state);
        }

    }

}