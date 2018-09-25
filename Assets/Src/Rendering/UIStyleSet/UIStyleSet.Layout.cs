using Src.Layout;

namespace Rendering {

    public partial class UIStyleSet {

        public GridPlacementParameters gridItem = new GridPlacementParameters(IntUtil.UnsetValue, 1, IntUtil.UnsetValue, 1);
        public GridDefinition gridDefinition;
        
        public LayoutParameters layoutParameters {
            get {
                return new LayoutParameters(
                    layoutType,
                    layoutWrap,
                    layoutFlow,
                    layoutDirection,
                    mainAxisAlignment,
                    crossAxisAlignment
                );
            }
            set { SetLayout(value); }
        }

        public LayoutFlowType layoutFlow {
            get { return FindActiveStyle((s) => s.layoutParameters.flow != LayoutFlowType.Unset).layoutParameters.flow; }
            set { SetLayoutFlow(value); }
        }

        public LayoutDirection layoutDirection {
            get { return FindActiveStyle((s) => s.layoutParameters.direction != LayoutDirection.Unset).layoutParameters.direction; }
            set { SetLayoutDirection(value); }
        }

        public LayoutType layoutType {
            get { return FindActiveStyle((s) => s.layoutParameters.type != LayoutType.Unset).layoutParameters.type; }
            set { SetLayoutType(value); }
        }

        public LayoutWrap layoutWrap {
            get { return FindActiveStyle((s) => s.layoutParameters.wrap != LayoutWrap.Unset).layoutParameters.wrap; }
            set { SetLayoutWrap(value); }
        }

        public MainAxisAlignment mainAxisAlignment {
            get { return FindActiveStyle((s) => s.layoutParameters.mainAxisAlignment != MainAxisAlignment.Unset).layoutParameters.mainAxisAlignment; }
            set { SetMainAxisAlignment(value); }
        }

        public CrossAxisAlignment crossAxisAlignment {
            get { return FindActiveStyle((s) => s.layoutParameters.crossAxisAlignment != CrossAxisAlignment.Unset).layoutParameters.crossAxisAlignment; }
            set { SetCrossAxisAlignment(value); }
        }

        public LayoutParameters GetLayout(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutParameters;
        }

        public void SetLayout(LayoutParameters parameters, StyleState state = StyleState.Normal) {
            UIStyle style = GetOrCreateStyle(state);
            style.layoutParameters = parameters;
            if (layoutParameters == parameters) {
                changeHandler.SetLayout(element, layoutParameters);
            }
        }

        public LayoutDirection GetLayoutDirection(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutParameters.direction;
        }

        public void SetLayoutDirection(LayoutDirection direction, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutParameters.direction = direction;
            if (layoutDirection == direction) {
                changeHandler.SetLayout(element, layoutParameters);
            }
        }

        public LayoutFlowType GetLayoutFlow(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutParameters.flow;
        }

        public void SetLayoutFlow(LayoutFlowType flowType, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutParameters.flow = flowType;
            if (flowType == layoutFlow) {
                changeHandler.SetLayout(element, layoutParameters);
            }
        }

        public LayoutType GetLayoutType(StyleState state = StyleState.Normal) {
            return GetOrCreateStyle(state).layoutParameters.type;
        }

        public void SetLayoutType(LayoutType layoutType, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutParameters.type = layoutType;
            if (this.layoutType == layoutType) {
                changeHandler.SetLayout(element, layoutParameters);
            }
        }

        public LayoutWrap GetLayoutWrap(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutParameters.wrap;
        }

        public void SetLayoutWrap(LayoutWrap layoutWrap, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutParameters.wrap = layoutWrap;
            if (this.layoutWrap == layoutWrap) {
                changeHandler.SetLayout(element, layoutParameters);
            }
        }

        public MainAxisAlignment GetMainAxisAlignment(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutParameters.mainAxisAlignment;
        }

        public void SetMainAxisAlignment(MainAxisAlignment alignment, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutParameters.mainAxisAlignment = alignment;
            if (this.mainAxisAlignment == alignment) {
                changeHandler.SetLayout(element, layoutParameters);
            }
        }

        public CrossAxisAlignment GetCrossAxisAlignment(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutParameters.crossAxisAlignment;
        }

        public void SetCrossAxisAlignment(CrossAxisAlignment alignment, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutParameters.crossAxisAlignment = alignment;
            if (this.crossAxisAlignment == alignment) {
                changeHandler.SetLayout(element, layoutParameters);
            }
        }

    }

}