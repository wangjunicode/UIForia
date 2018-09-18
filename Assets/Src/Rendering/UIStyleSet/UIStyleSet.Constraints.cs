using JetBrains.Annotations;
using Src;
using Src.Systems;

namespace Rendering {

    [PublicAPI]
    public partial class UIStyleSet {

        [PublicAPI]
        public LayoutConstraints constraints {
            get { return new LayoutConstraints(minWidth, maxWidth, minHeight, maxHeight, growthFactor, shrinkFactor); }
            set { SetLayoutConstraints(value); }
        }

        [PublicAPI]
        public UIMeasurement minWidth {
            get { return FindActiveStyle((s) => s.layoutConstraints.minWidth.IsDefined()).layoutConstraints.minWidth; }
            set { SetMinWidth(value); }
        }

        [PublicAPI]
        public UIMeasurement maxWidth {
            get { return FindActiveStyle((s) => s.layoutConstraints.maxWidth.IsDefined()).layoutConstraints.maxWidth; }
            set { SetMaxWidth(value); }
        }

        [PublicAPI]
        public UIMeasurement minHeight {
            get { return FindActiveStyle((s) => s.layoutConstraints.minHeight.IsDefined()).layoutConstraints.minHeight; }
            set { SetMinHeight(value); }
        }

        [PublicAPI]
        public UIMeasurement maxHeight {
            get { return FindActiveStyle((s) => s.layoutConstraints.maxHeight.IsDefined()).layoutConstraints.maxHeight; }
            set { SetMaxHeight(value); }
        }

        [PublicAPI]
        public int growthFactor {
            get { return FindActiveStyle((s) => s.layoutConstraints.growthFactor != IntUtil.UnsetValue).layoutConstraints.growthFactor; }
            set { SetGrowthFactor(value); }
        }

        [PublicAPI]
        public int shrinkFactor {
            get { return FindActiveStyle((s) => s.layoutConstraints.shrinkFactor != IntUtil.UnsetValue).layoutConstraints.shrinkFactor; }
            set { SetShrinkFactor(value); }
        }

        [PublicAPI]
        public LayoutConstraints GetConstraints(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutConstraints;
        }

        [PublicAPI]
        public void SetLayoutConstraints(LayoutConstraints value, StyleState state = StyleState.Normal) {
            UIStyle style = GetOrCreateStyle(state);
            style.layoutConstraints = value;
            changeHandler.SetConstraints(element, constraints);
        }

        [PublicAPI]
        public UIMeasurement GetMinWidth(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutConstraints.minWidth;
        }

        [PublicAPI]
        public void SetMinWidth(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutConstraints.minWidth = measurement;
            if (minWidth == measurement) {
                changeHandler.SetConstraints(element, constraints);
            }
        }

        [PublicAPI]
        public UIMeasurement GetMaxWidth(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutConstraints.maxWidth;
        }

        [PublicAPI]
        public void SetMaxWidth(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutConstraints.maxWidth = measurement;
            if (maxWidth == measurement) {
                changeHandler.SetConstraints(element, constraints);
            }
        }

        [PublicAPI]
        public UIMeasurement GetMinHeight(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutConstraints.minHeight;
        }

        [PublicAPI]
        public void SetMinHeight(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutConstraints.minHeight = measurement;
            if (minHeight == measurement) {
                changeHandler.SetConstraints(element, constraints);
            }
        }

        [PublicAPI]
        public UIMeasurement GetMaxHeight(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutConstraints.maxHeight;
        }

        [PublicAPI]
        public void SetMaxHeight(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutConstraints.maxHeight = measurement;
            if (maxHeight == measurement) {
                changeHandler.SetConstraints(element, constraints);
            }
        }

        [PublicAPI]
        public int GetShrinkFactor(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutConstraints.shrinkFactor;
        }

        [PublicAPI]
        public void SetShrinkFactor(int factor, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutConstraints.shrinkFactor = factor;
            if (shrinkFactor == factor) {
                changeHandler.SetConstraints(element, constraints);
            }
        }

        [PublicAPI]
        public int GetGrowthFactor(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutConstraints.growthFactor;
        }

        [PublicAPI]
        public void SetGrowthFactor(int factor, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutConstraints.growthFactor = factor;
            if (growthFactor == factor) {
                changeHandler.SetConstraints(element, constraints);
            }
        }

    }

}