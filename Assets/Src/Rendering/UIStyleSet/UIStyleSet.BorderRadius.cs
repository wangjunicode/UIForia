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
        public UIFixedLength GetBorderRadiusTopLeft(StyleState state) {
            return GetFixedLengthValue(StylePropertyId.BorderRadiusTopLeft, state);
        }

        [PublicAPI]
        public UIFixedLength GetBorderRadiusTopRight(StyleState state) {
            return GetFixedLengthValue(StylePropertyId.BorderRadiusTopRight, state);
        }

        [PublicAPI]
        public UIFixedLength GetBorderRadiusBottomLeft(StyleState state) {
            return GetFixedLengthValue(StylePropertyId.BorderRadiusBottomLeft, state);
        }

        [PublicAPI]
        public UIFixedLength GetBorderRadiusBottomRight(StyleState state) {
            return GetFixedLengthValue(StylePropertyId.BorderRadiusBottomRight, state);
        }

        [PublicAPI]
        public void SetBorderRadiusTopLeft(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.BorderRadiusTopLeft, value, state);
        }

        [PublicAPI]
        public void SetBorderRadiusTopRight(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.BorderRadiusTopRight, value, state);
        }

        [PublicAPI]
        public void SetBorderRadiusBottomRight(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.BorderRadiusBottomRight, value, state);
        }

        [PublicAPI]
        public void SetBorderRadiusBottomLeft(UIFixedLength value, StyleState state) {
            SetFixedLengthProperty(StylePropertyId.BorderRadiusBottomLeft, value, state);
        }

    }

}