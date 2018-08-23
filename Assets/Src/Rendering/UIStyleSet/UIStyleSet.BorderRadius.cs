using JetBrains.Annotations;

namespace Rendering {

    public partial class UIStyleSet {

        [PublicAPI]
        public BorderRadius borderRadius {
            get { return new BorderRadius(borderRadiusTopLeft, borderRadiusTopRight, borderRadiusBottomRight, borderRadiusBottomLeft); }
            set { SetBorderRadius(value, StyleState.Normal); }
        }

        [PublicAPI]
        public float borderRadiusTopLeft {
            get { return FindActiveStyle((s) => s.borderRadius.HasTopLeft).borderRadius.topLeft; }
            set { SetBorderRadiusTopLeft(value, StyleState.Normal); }
        }

        [PublicAPI]
        public float borderRadiusTopRight {
            get { return FindActiveStyle((s) => s.borderRadius.HasTopRight).borderRadius.topRight; }
            set { SetBorderRadiusTopRight(value, StyleState.Normal); }
        }

        [PublicAPI]
        public float borderRadiusBottomRight {
            get { return FindActiveStyle((s) => s.borderRadius.HasBottomRight).borderRadius.bottomRight; }
            set { SetBorderRadiusBottomRight(value, StyleState.Normal); }
        }

        [PublicAPI]
        public float borderRadiusBottomLeft {
            get { return FindActiveStyle((s) => s.borderRadius.HasBottomLeft).borderRadius.bottomLeft; }
            set { SetBorderRadiusBottomLeft(value, StyleState.Normal); }
        }

        [PublicAPI]
        public BorderRadius GetBorderRadius(StyleState state) {
            return GetStyle(state).borderRadius;
        }

        [PublicAPI]
        public void SetBorderRadius(BorderRadius newBorderRadius, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.borderRadius = newBorderRadius;
            changeHandler.SetBorderRadius(elementId, borderRadius);
        }

        [PublicAPI]
        public float GetBorderRadiusTopLeft(StyleState state) {
            return GetStyle(state).borderRadius.topLeft;
        }

        [PublicAPI]
        public void SetBorderRadiusTopLeft(float value, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.borderRadius = new BorderRadius(
                value,
                style.borderRadius.topRight,
                style.borderRadius.bottomRight,
                style.borderRadius.bottomLeft
            );
            if (borderRadiusTopLeft == value) {
                changeHandler.SetBorderRadius(elementId, borderRadius);
            }
        }

        [PublicAPI]
        public float GetBorderRadiusTopRight(StyleState state) {
            return GetStyle(state).borderRadius.topRight;
        }

        [PublicAPI]
        public void SetBorderRadiusTopRight(float value, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.borderRadius = new BorderRadius(
                style.borderRadius.topLeft,
                value,
                style.borderRadius.bottomRight,
                style.borderRadius.bottomLeft
            );
            if (borderRadiusTopRight == value) {
                changeHandler.SetBorderRadius(elementId, borderRadius);
            }
        }

        [PublicAPI]
        public float GetBorderRadiusBottomRight(StyleState state) {
            return GetStyle(state).borderRadius.bottomRight;
        }

        [PublicAPI]
        public void SetBorderRadiusBottomRight(float value, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.borderRadius = new BorderRadius(
                style.borderRadius.topLeft,
                style.borderRadius.topRight,
                value,
                style.borderRadius.bottomLeft
            );
            if (borderRadiusBottomRight == value) {
                changeHandler.SetBorderRadius(elementId, borderRadius);
            }
        }

        [PublicAPI]
        public float GetBorderRadiusBottomLeft(StyleState state) {
            return GetStyle(state).borderRadius.bottomLeft;
        }

        [PublicAPI]
        public void SetBorderRadiusBottomLeft(float value, StyleState state) {
            UIStyle style = GetOrCreateStyle(state);
            style.borderRadius = new BorderRadius(
                style.borderRadius.topLeft,
                style.borderRadius.topRight,
                style.borderRadius.bottomRight,
                value
            );
            if (borderRadiusBottomLeft == value) {
                changeHandler.SetBorderRadius(elementId, borderRadius);
            }
        }

    }

}