using JetBrains.Annotations;
using Src;

namespace Rendering {

    public partial class UIStyleSet {

        [PublicAPI]
        public Dimensions dimensions {
            get { return computedStyle.dimensions; }
            set { SetDimensions(value, StyleState.Normal); }
        }

        [PublicAPI]
        public UIMeasurement width {
            get { return computedStyle.dimensions.width; }
            set { SetWidth(value, StyleState.Normal); }
        }

        [PublicAPI]
        public UIMeasurement height {
            get { return computedStyle.dimensions.height; }
            set { SetHeight(value, StyleState.Normal); }
        }

        [PublicAPI]
        public Dimensions GetDimensions(StyleState state = StyleState.Normal) {
            return GetStyle(state).dimensions;
        }

        [PublicAPI]
        public void SetDimensions(Dimensions newDimensions, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.dimensions = newDimensions;
            if ((state & currentState) != 0) {
                bool widthMatch = style == FindActiveStyleWithoutDefault((s) => s.dimensions.width.IsDefined());
                bool heightMatch = style == FindActiveStyleWithoutDefault((s) => s.dimensions.width.IsDefined());
                if (widthMatch && heightMatch) {
                    computedStyle.dimensions = newDimensions;
                    changeHandler.SetDimensions(element, newDimensions);
                }
                else if (widthMatch) {
                    computedStyle.dimensions = new Dimensions(newDimensions.width, computedStyle.dimensions.height);
                    changeHandler.SetDimensions(element, computedStyle.dimensions);
                }
                else if (heightMatch) {
                    computedStyle.dimensions = new Dimensions(computedStyle.dimensions.width, newDimensions.height);
                    changeHandler.SetDimensions(element, computedStyle.dimensions);
                }
            }
        }

        [PublicAPI]
        public UIMeasurement GetWidth(StyleState state) {
            return GetStyle(state).dimensions.width;
        }

        [PublicAPI]
        public void SetWidth(UIMeasurement width, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.dimensions = new Dimensions(width, style.dimensions.height);
            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => s.dimensions.width.IsDefined())) {
                computedStyle.dimensions = new Dimensions(width, computedStyle.dimensions.height);
                changeHandler.SetDimensions(element, dimensions);
            }
        }

        [PublicAPI]
        public UIMeasurement GetHeight(StyleState state) {
            return GetStyle(state).dimensions.height;
        }

        [PublicAPI]
        public void SetHeight(UIMeasurement newHeight, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.dimensions = new Dimensions(style.dimensions.width, newHeight);
            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => s.dimensions.height.IsDefined())) {
                computedStyle.dimensions = new Dimensions(computedStyle.dimensions.width, newHeight);
                changeHandler.SetDimensions(element, dimensions);
            }
        }

    }

}