using JetBrains.Annotations;
using Src;

namespace Rendering {

    public partial class UIStyleSet {

        [PublicAPI]
        public LayoutRect rect {
            get { return new LayoutRect(rectX, rectY, rectWidth, rectHeight); }
            set { SetRect(value); }
        }

        [PublicAPI]
        public UIMeasurement rectX {
            get { return FindActiveStyle((s) => s.rect.x != UIStyle.UnsetMeasurementValue).rect.x; }
            set { SetRectX(value); }
        }

        [PublicAPI]
        public UIMeasurement rectY {
            get { return FindActiveStyle((s) => s.rect.y != UIStyle.UnsetMeasurementValue).rect.y; }
            set { SetRectY(value); }
        }

        [PublicAPI]
        public UIMeasurement rectWidth {
            get { return FindActiveStyle((s) => s.rect.width != UIStyle.UnsetMeasurementValue).rect.width; }
            set { SetRectWidth(value); }
        }

        [PublicAPI]
        public UIMeasurement rectHeight {
            get { return FindActiveStyle((s) => s.rect.height != UIStyle.UnsetMeasurementValue).rect.height; }
            set { SetRectHeight(value); }
        }

        [PublicAPI]
        public LayoutRect GetRect(StyleState state = StyleState.Normal) {
            return GetStyle(state).rect;
        }

        [PublicAPI]
        public void SetRect(LayoutRect newRect, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).rect = newRect;
            changeHandler.SetRect(elementId, newRect);
        }

        [PublicAPI]
        public UIMeasurement GetRectX(StyleState state) {
            return GetStyle(state).rect.x;
        }

        [PublicAPI]
        public void SetRectX(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            UIStyle style = GetOrCreateStyle(state);
            style.rect = new LayoutRect(measurement, style.rect.y, style.rect.width, style.rect.height);
            if (rectX == measurement) {
                changeHandler.SetRect(elementId, rect);
            }
        }

        [PublicAPI]
        public UIMeasurement GetRectY(StyleState state) {
            return GetStyle(state).rect.y;
        }

        [PublicAPI]
        public void SetRectY(UIMeasurement measurement, StyleState state = StyleState.Normal) {
            UIStyle style = GetOrCreateStyle(state);
            style.rect = new LayoutRect(style.rect.x, measurement, style.rect.width, style.rect.height);
            if (rectY == measurement) {
                changeHandler.SetRect(elementId, rect);
            }
        }

        [PublicAPI]
        public UIMeasurement GetRectWidth(StyleState state) {
            return GetStyle(state).rect.width;
        }

        [PublicAPI]
        public void SetRectWidth(UIMeasurement width, StyleState state = StyleState.Normal) {
            UIStyle style = GetOrCreateStyle(state);
            style.rect = new LayoutRect(style.rect.x, style.rect.y, width, style.rect.height);
            if (rectWidth == width) {
                changeHandler.SetRect(elementId, rect);
            }
        }

        [PublicAPI]
        public UIMeasurement GetRectHeight(StyleState state) {
            return GetStyle(state).rect.height;
        }

        [PublicAPI]
        public void SetRectHeight(UIMeasurement height, StyleState state = StyleState.Normal) {
            UIStyle style = GetOrCreateStyle(state);
            style.rect = new LayoutRect(style.rect.x, style.rect.y, style.rect.width, height);
            if (rectHeight == height) {
                changeHandler.SetRect(elementId, rect);
            }
        }

    }

}