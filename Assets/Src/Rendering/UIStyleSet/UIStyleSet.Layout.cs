using Src.Layout;

namespace Rendering {

    public partial class UIStyleSet {

        public LayoutType GetLayoutType(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.LayoutType, state);
            return property.IsDefined ? (LayoutType) property.valuePart0 : LayoutType.Unset;
        }

        public void SetLayoutType(LayoutType layoutType, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.LayoutType = layoutType;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.LayoutType)) {
                computedStyle.LayoutType = layoutType;
            }
        }

        public LayoutBehavior GetLayoutBehavior(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.LayoutBehavior, state);
            return property.IsDefined ? (LayoutBehavior) property.valuePart0 : LayoutBehavior.Unset;
        }

        public void SetLayoutBehavior(LayoutBehavior behavior, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.LayoutBehavior = behavior;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.LayoutBehavior)) {
                computedStyle.LayoutBehavior = behavior;
            }
        }
    }

}
//
//        public GridPlacementParameters gridItem = new GridPlacementParameters(IntUtil.UnsetValue, 1, IntUtil.UnsetValue, 1);
//        public GridDefinition gridDefinition;
//
//        public LayoutParameters layoutParameters {
//            get {
//                return new LayoutParameters(
//                    layoutType,
//                    layoutWrap,
//                    layoutFlow,
//                    layoutDirection,
//                    mainAxisAlignment,
//                    crossAxisAlignment
//                );
//            }
//            set { SetLayout(value, StyleState.Normal); }
//        }
//
//        public LayoutFlowType layoutFlow {
//            get { return FindActiveStyle((s) => s.layoutParameters.flow != LayoutFlowType.Unset).layoutParameters.flow; }
//            set { SetLayoutFlow(value, StyleState.Normal); }
//        }
//
//        public LayoutDirection layoutDirection {
//            get { return FindActiveStyle((s) => s.layoutParameters.direction != LayoutDirection.Unset).layoutParameters.direction; }
//            set { SetLayoutDirection(value, StyleState.Normal); }
//        }
//
//        public LayoutType layoutType {
//            get { return FindActiveStyle((s) => s.layoutParameters.type != LayoutType.Unset).layoutParameters.type; }
//            set { SetLayoutType(value, StyleState.Normal); }
//        }
//
//        public LayoutWrap layoutWrap {
//            get { return FindActiveStyle((s) => s.layoutParameters.wrap != LayoutWrap.Unset).layoutParameters.wrap; }
//            set { SetLayoutWrap(value, StyleState.Normal); }
//        }
//
//        public MainAxisAlignment mainAxisAlignment {
//            get { return FindActiveStyle((s) => s.layoutParameters.mainAxisAlignment != MainAxisAlignment.Unset).layoutParameters.mainAxisAlignment; }
//            set { SetMainAxisAlignment(value, StyleState.Normal); }
//        }
//
//        public CrossAxisAlignment crossAxisAlignment {
//            get { return FindActiveStyle((s) => s.layoutParameters.crossAxisAlignment != CrossAxisAlignment.Unset).layoutParameters.crossAxisAlignment; }
//            set { SetCrossAxisAlignment(value, StyleState.Normal); }
//        }
//
//        public LayoutParameters GetLayout(StyleState state) {
//            return GetStyle(state).layoutParameters;
//        }
//
//        public void SetLayout(LayoutParameters parameters, StyleState state) {
//            UIStyle style = GetOrCreateInstanceStyle(state);
//            style.layoutParameters = parameters;
//            if (layoutParameters == parameters) {
//                styleSystem.SetLayout(element, layoutParameters);
//            }
//        }
//
//        public LayoutDirection GetLayoutDirection(StyleState state) {
//            return GetStyle(state).layoutParameters.direction;
//        }
//
//        public void SetLayoutDirection(LayoutDirection direction, StyleState state) {
//            GetOrCreateInstanceStyle(state).layoutParameters.direction = direction;
//            if (layoutDirection == direction) {
//                styleSystem.SetLayout(element, layoutParameters);
//            }
//        }
//
//        public LayoutFlowType GetLayoutFlow(StyleState state) {
//            return GetStyle(state).layoutParameters.flow;
//        }
//
//        public void SetLayoutFlow(LayoutFlowType flowType, StyleState state) {
//            GetOrCreateInstanceStyle(state).layoutParameters.flow = flowType;
//            if (flowType == layoutFlow) {
//                styleSystem.SetLayout(element, layoutParameters);
//            }
//        }
//
//        public LayoutType GetLayoutType(StyleState state) {
//            return GetOrCreateInstanceStyle(state).layoutParameters.type;
//        }
//
//        public void SetLayoutType(LayoutType layoutType, StyleState state) {
//            GetOrCreateInstanceStyle(state).layoutParameters.type = layoutType;
//            if (this.layoutType == layoutType) {
//                styleSystem.SetLayout(element, layoutParameters);
//            }
//        }
//
//        public LayoutWrap GetLayoutWrap(StyleState state) {
//            return GetStyle(state).layoutParameters.wrap;
//        }
//
//        public void SetLayoutWrap(LayoutWrap layoutWrap, StyleState state) {
//            UIStyle style = GetOrCreateInstanceStyle(state);
//            style.layoutParameters.wrap = layoutWrap;
//            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => s.layoutParameters.wrap != LayoutWrap.Unset)) {
//                LayoutWrap oldWrap = computedStyle.layoutParameters.wrap;
//                computedStyle.layoutParameters.wrap = layoutWrap;
//                styleSystem.SetLayoutWrap(element, layoutWrap, oldWrap);
//            }
//        }
//
//        public MainAxisAlignment GetMainAxisAlignment(StyleState state) {
//            return GetStyle(state).layoutParameters.mainAxisAlignment;
//        }
//
//        public void SetMainAxisAlignment(MainAxisAlignment alignment, StyleState state) {
//            UIStyle style = GetOrCreateInstanceStyle(state);
//            style.layoutParameters.mainAxisAlignment = alignment;
//            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => s.layoutParameters.mainAxisAlignment != MainAxisAlignment.Unset)) {
//                MainAxisAlignment oldAlignment = computedStyle.layoutParameters.mainAxisAlignment;
//                computedStyle.layoutParameters.mainAxisAlignment = alignment;
//                styleSystem.SetMainAxisAlignment(element, alignment, oldAlignment);
//            }
//        }
//
//        public CrossAxisAlignment GetCrossAxisAlignment(StyleState state) {
//            return GetStyle(state).layoutParameters.crossAxisAlignment;
//        }
//
//        public void SetCrossAxisAlignment(CrossAxisAlignment alignment, StyleState state) {
//            UIStyle style = GetOrCreateInstanceStyle(state);
//            style.layoutParameters.crossAxisAlignment = alignment;
//            if ((state & currentState) != 0 && style == FindActiveStyleWithoutDefault((s) => s.layoutParameters.crossAxisAlignment != CrossAxisAlignment.Unset)) {
//                CrossAxisAlignment oldAlignment = computedStyle.layoutParameters.crossAxisAlignment;
//                computedStyle.layoutParameters.crossAxisAlignment = alignment;
//                styleSystem.SetCrossAxisAlignment(element, alignment, oldAlignment);
//            }
//        }
//
//    }
//
//}