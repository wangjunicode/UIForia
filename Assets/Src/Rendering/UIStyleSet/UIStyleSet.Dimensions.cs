using JetBrains.Annotations;
using Src;

namespace Rendering {

    public partial class UIStyleSet {

        [PublicAPI]
        public Dimensions dimensions {
            get { return new Dimensions(width, height); }
            set { SetDimensions(value); }
        }
//
//        [PublicAPI]
//        public UIMeasurement rectX {
//            get { return FindActiveStyle((s) => s.dimensions.x != UIMeasurement.Unset).dimensions.x; }
//            set { SetRectX(value); }
//        }
//
//        [PublicAPI]
//        public UIMeasurement rectY {
//            get { return FindActiveStyle((s) => s.dimensions.y != UIMeasurement.Unset).dimensions.y; }
//            set { SetRectY(value); }
//        }

        [PublicAPI]
        public UIMeasurement width {
            get { return FindActiveStyle((s) => s.dimensions.width != UIMeasurement.Unset).dimensions.width; }
            set { SetWidth(value); }
        }

        [PublicAPI]
        public UIMeasurement height {
            get { return FindActiveStyle((s) => s.dimensions.height != UIMeasurement.Unset).dimensions.height; }
            set { SetHeight(value); }
        }

        [PublicAPI]
        public Dimensions GetDimensions(StyleState state = StyleState.Normal) {
            return GetStyle(state).dimensions;
        }

        [PublicAPI]
        public void SetDimensions(Dimensions newDimensions, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).dimensions = newDimensions;
            changeHandler.SetDimensions(elementId, newDimensions);
        }

//        [PublicAPI]
//        public UIMeasurement GetRectX(StyleState state) {
//            return GetStyle(state).dimensions.x;
//        }
//
//        [PublicAPI]
//        public void SetRectX(UIMeasurement measurement, StyleState state = StyleState.Normal) {
//            UIStyle style = GetOrCreateStyle(state);
//            style.dimensions = new Dimensions(measurement, style.dimensions.y, style.dimensions.width, style.dimensions.height);
//            if (rectX == measurement) {
//                changeHandler.SetRect(elementId, rect);
//            }
//        }
//
//        [PublicAPI]
//        public UIMeasurement GetRectY(StyleState state) {
//            return GetStyle(state).dimensions.y;
//        }
//
//        [PublicAPI]
//        public void SetRectY(UIMeasurement measurement, StyleState state = StyleState.Normal) {
//            UIStyle style = GetOrCreateStyle(state);
//            style.dimensions = new Dimensions(style.dimensions.x, measurement, style.dimensions.width, style.dimensions.height);
//            if (rectY == measurement) {
//                changeHandler.SetRect(elementId, rect);
//            }
//        }

        [PublicAPI]
        public UIMeasurement GetWidth(StyleState state) {
            return GetStyle(state).dimensions.width;
        }

        [PublicAPI]
        public void SetWidth(UIMeasurement width, StyleState state = StyleState.Normal) {
            UIStyle style = GetOrCreateStyle(state);
            style.dimensions = new Dimensions(width, style.dimensions.height);
            if (this.width == width) {
                changeHandler.SetDimensions(elementId, dimensions);
            }
        }

        [PublicAPI]
        public UIMeasurement GetHeight(StyleState state) {
            return GetStyle(state).dimensions.height;
        }

        [PublicAPI]
        public void SetHeight(UIMeasurement height, StyleState state = StyleState.Normal) {
            UIStyle style = GetOrCreateStyle(state);
            style.dimensions = new Dimensions(style.dimensions.width, height);
            if (this.height == height) {
                changeHandler.SetDimensions(elementId, dimensions);
            }
        }

    }

}