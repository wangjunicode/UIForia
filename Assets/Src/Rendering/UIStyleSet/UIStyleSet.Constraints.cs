using Src;

namespace Rendering {

    public partial class UIStyleSet {

        public UIMeasurement minWidth {
            get { return FindActiveStyle((s) => s.minWidth != UIStyle.UnsetMeasurementValue).minWidth; }
            set { SetMinWidth(value); }
        }

        public UIMeasurement maxWidth {
            get { return FindActiveStyle((s) => s.maxWidth != UIStyle.UnsetMeasurementValue).maxWidth; }
            set { SetMaxWidth(value); }
        }

        public UIMeasurement minHeight {
            get { return FindActiveStyle((s) => s.minHeight != UIStyle.UnsetMeasurementValue).minHeight; }
            set { SetMinHeight(value); }
        }

        public UIMeasurement maxHeight {
            get { return FindActiveStyle((s) => s.maxHeight != UIStyle.UnsetMeasurementValue).maxHeight; }
            set { SetMaxHeight(value); }
        }

        public int growthFactor {
            get { return FindActiveStyle((s) => s.growthFactor != UIStyle.UnsetIntValue).growthFactor; }
            set { SetGrowthFactor(value); }
        }
        
        public int shrinkFactor {
            get { return FindActiveStyle((s) => s.shrinkFactor != UIStyle.UnsetIntValue).shrinkFactor; }
            set { SetShrinkFactor(value); }
        }
        
        public UIMeasurement GetMinWidth(StyleState state = StyleState.Normal) {
            return GetStyle(state).minWidth;
        }

        public void SetMinWidth(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).minWidth = measurement;
            if (minWidth == measurement) {
                view.layoutSystem.SetRectMinWidth(element, measurement);
            }
        }

        public UIMeasurement GetMaxWidth(StyleState state = StyleState.Normal) {
            return GetStyle(state).maxWidth;
        }

        public void SetMaxWidth(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).maxWidth = measurement;
            if (maxWidth == measurement) {
                view.layoutSystem.SetRectMaxWidth(element, measurement);
            }
        }

        public UIMeasurement GetMinHeight(StyleState state = StyleState.Normal) {
            return GetStyle(state).minHeight;
        }

        public void SetMinHeight(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).minHeight = measurement;
            if (minHeight == measurement) {
                view.layoutSystem.SetRectMinHeight(element, measurement);
            }
        }

        public UIMeasurement GetMaxHeight(StyleState state = StyleState.Normal) {
            return GetStyle(state).maxHeight;
        }

        public void SetMaxHeight(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).maxHeight = measurement;
            if (maxHeight == measurement) {
                view.layoutSystem.SetRectMaxHeight(element, measurement);
            }
        }

        public int GetShrinkFactor(StyleState state = StyleState.Normal) {
            return GetStyle(state).shrinkFactor;
        }

        public void SetShrinkFactor(int factor, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).shrinkFactor = factor;
            if (shrinkFactor == factor) {
                view.layoutSystem.SetShrinkFactor(element, factor);
            }
        }

        public int GetGrowthFactor(StyleState state = StyleState.Normal) {
            return GetStyle(state).growthFactor;
        }

        public void SetGrowthFactor(int factor, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).growthFactor = factor;
            if (growthFactor == factor) {
                view.layoutSystem.SetGrowthFactor(element, factor);
            }
        }

    }

}