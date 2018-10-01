using JetBrains.Annotations;
using Src;

namespace Rendering {

    [PublicAPI]
    public partial class UIStyleSet {

        [PublicAPI]
        public UIMeasurement GetMinWidth(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.MinWidth, state);
        }

        [PublicAPI]
        public UIMeasurement GetMaxWidth(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.MaxWidth, state);
        }

        [PublicAPI]
        public UIMeasurement GetPreferredWidth(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.PreferredWidth, state);
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
            return GetUIMeasurementValue(StylePropertyId.MinHeight, state);
        }

        [PublicAPI]
        public UIMeasurement GetMaxHeight(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.MaxHeight, state);
        }

        [PublicAPI]
        public UIMeasurement GetPreferredHeight(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.PreferredHeight, state);
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