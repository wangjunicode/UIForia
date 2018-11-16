//using JetBrains.Annotations;
//using UIForia;
//
//namespace UIForia.Rendering {
//
//    public partial class UIStyleSet {
//
//        [PublicAPI]
//        public BorderRadius GetBorderRadius(StyleState state) {
//            return new BorderRadius(
//                GetBorderRadiusTopLeft(state),
//                GetBorderRadiusTopRight(state),
//                GetBorderRadiusBottomRight(state),
//                GetBorderRadiusBottomLeft(state)
//            );
//        }
//
//        [PublicAPI]
//        public void SetBorderRadius(BorderRadius newBorderRadius, StyleState state) {
//            SetBorderRadiusBottomLeft(newBorderRadius.bottomLeft, state);
//            SetBorderRadiusBottomRight(newBorderRadius.bottomRight, state);
//            SetBorderRadiusTopRight(newBorderRadius.topRight, state);
//            SetBorderRadiusTopLeft(newBorderRadius.topLeft, state);
//        }
//
//        [PublicAPI]
//        public UIFixedLength GetBorderRadiusTopLeft(StyleState state) {
//            return GetPropertyValueInState(StylePropertyId.BorderRadiusTopLeft, state).AsUIFixedLength;
//        }
//
//        [PublicAPI]
//        public UIFixedLength GetBorderRadiusTopRight(StyleState state) {
//            return GetPropertyValueInState(StylePropertyId.BorderRadiusTopRight, state).AsUIFixedLength;
//        }
//
//        [PublicAPI]
//        public UIFixedLength GetBorderRadiusBottomLeft(StyleState state) {
//            return GetPropertyValueInState(StylePropertyId.BorderRadiusBottomLeft, state).AsUIFixedLength;
//        }
//
//        [PublicAPI]
//        public UIFixedLength GetBorderRadiusBottomRight(StyleState state) {
//            return GetPropertyValueInState(StylePropertyId.BorderRadiusBottomRight, state).AsUIFixedLength;
//        }
//
//        [PublicAPI]
//        public void SetBorderRadiusTopLeft(UIFixedLength value, StyleState state) {
//            SetFixedLengthProperty(StylePropertyId.BorderRadiusTopLeft, value, state);
//        }
//
//        [PublicAPI]
//        public void SetBorderRadiusTopRight(UIFixedLength value, StyleState state) {
//            SetFixedLengthProperty(StylePropertyId.BorderRadiusTopRight, value, state);
//        }
//
//        [PublicAPI]
//        public void SetBorderRadiusBottomRight(UIFixedLength value, StyleState state) {
//            SetFixedLengthProperty(StylePropertyId.BorderRadiusBottomRight, value, state);
//        }
//
//        [PublicAPI]
//        public void SetBorderRadiusBottomLeft(UIFixedLength value, StyleState state) {
//            SetFixedLengthProperty(StylePropertyId.BorderRadiusBottomLeft, value, state);
//        }
//
//    }
//
//}