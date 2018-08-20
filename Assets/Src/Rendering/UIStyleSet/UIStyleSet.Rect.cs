using Src;

namespace Rendering {

    public partial class UIStyleSet {

        #region Rect

        public UIMeasurement rectX {
            get { return FindActiveStyle((s) => s.rect.x != UIStyle.UnsetMeasurementValue).rect.x; }
            set { SetRectX(value); }
        }

        public UIMeasurement rectY {
            get { return FindActiveStyle((s) => s.rect.y != UIStyle.UnsetMeasurementValue).rect.y; }
            set { SetRectY(value); }
        }

        public UIMeasurement rectWidth {
            get { return FindActiveStyle((s) => s.rect.width != UIStyle.UnsetMeasurementValue).rect.width; }
            set { SetRectWidth(value); }
        }

        public UIMeasurement rectHeight {
            get { return FindActiveStyle((s) => s.rect.height != UIStyle.UnsetMeasurementValue).rect.height; }
            set { SetRectHeight(value); }
        }

        public UIMeasurement GetRectX(StyleState state) {
            return GetStyle(state).rect.x;
        }

        public void SetRectX(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).rect.x = measurement;
            if (rectX == measurement) {
                view.layoutSystem.SetRectX(element, measurement);
            }
        }

        public UIMeasurement GetRectY(StyleState state) {
            return GetStyle(state).rect.y;
        }

        public void SetRectY(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).rect.y = measurement;
            if (rectY == measurement) {
                view.layoutSystem.SetRectY(element, measurement);
            }
        }

        public UIMeasurement GetRectWidth(StyleState state) {
            return GetStyle(state).rect.width;
        }

        public void SetRectWidth(UIMeasurement width, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).rect.width = width;
            if (rectWidth == width) {
                view.layoutSystem.SetRectWidth(element, rectWidth);
            }
        }

        public UIMeasurement GetRectHeight(StyleState state) {
            return GetStyle(state).rect.height;
        }

        public void SetRectHeight(UIMeasurement height, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).rect.height = height;
            if (rectHeight == height) {
                view.layoutSystem.SetRectHeight(element, rectWidth);
            }
        }

        #endregion

    }

}