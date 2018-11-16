//using UIForia.Layout;
//
//namespace UIForia.Rendering {
//
//    public partial class UIStyleSet {
//
//        public LayoutWrap GetFlexWrapMode(StyleState state) {
//            return GetPropertyValueInState(StylePropertyId.FlexLayoutWrap, state).AsLayoutWrap;
//        }
//
//        public LayoutDirection GetFlexLayoutDirection(StyleState state) {
//            return GetPropertyValueInState(StylePropertyId.FlexLayoutDirection, state).AsLayoutDirection;
//        }
//
//        public MainAxisAlignment GetFlexLayoutMainAlignment(StyleState state) {
//            return GetPropertyValueInState(StylePropertyId.FlexLayoutMainAxisAlignment, state).AsMainAxisAlignment;
//        }
//
//        public CrossAxisAlignment GetFlexLayoutCrossAlignment(StyleState state) {
//            return GetPropertyValueInState(StylePropertyId.FlexLayoutCrossAxisAlignment, state).AsCrossAxisAlignment;
//        }
//
//        public void SetFlexWrapMode(LayoutWrap wrapMode, StyleState state) {
//            SetEnumProperty(StylePropertyId.FlexLayoutWrap, (int) wrapMode, state);
//        }
//
//        public void SetFlexDirection(LayoutDirection direction, StyleState state) {
//            SetEnumProperty(StylePropertyId.FlexLayoutDirection, (int) direction, state);
//        }
//
//        public void SetFlexMainAxisAlignment(MainAxisAlignment alignment, StyleState state) {
//            SetEnumProperty(StylePropertyId.FlexLayoutMainAxisAlignment, (int) alignment, state);
//        }
//
//        public void SetFlexCrossAxisAlignment(CrossAxisAlignment alignment, StyleState state) {
//            SetEnumProperty(StylePropertyId.FlexLayoutCrossAxisAlignment, (int) alignment, state);
//        }
//
//    }
//
//}