using Src.Layout;

namespace Rendering {

    public partial class UIStyleSet {

        public LayoutFlowType layoutFlow {
            get { return FindActiveStyle((s) => s.layoutFlow != LayoutFlowType.Unset).layoutFlow; }
            set { SetLayoutFlow(value); }
        }

        public LayoutDirection layoutDirection {
            get { return FindActiveStyle((s) => s.layoutDirection != LayoutDirection.Unset).layoutDirection; }
            set { SetLayoutDirection(value); }
        }

        public LayoutType layoutType {
            get { return FindActiveStyle((s) => s.layoutType != LayoutType.Unset).layoutType; }
            set { SetLayoutType(value); }
        }

        public LayoutWrap layoutWrap {
            get { return FindActiveStyle((s) => s.layoutWrap != LayoutWrap.Unset).layoutWrap; }
            set { SetLayoutWrap(value); }
        }

        public MainAxisAlignment mainAxisAlignment {
            get { return FindActiveStyle((s) => s.mainAxisAlignment != MainAxisAlignment.Unset).mainAxisAlignment; }
            set { SetMainAxisAlignment(value); }
        }

        public CrossAxisAlignment crossAxisAlignment {
            get { return FindActiveStyle((s) => s.crossAxisAlignment != CrossAxisAlignment.Unset).crossAxisAlignment; }
            set { SetCrossAxisAlignment(value); }
        }

        public LayoutDirection GetLayoutDirection(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutDirection;
        }

        public void SetLayoutDirection(LayoutDirection direction, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutDirection = direction;
            if (layoutDirection == direction) {
                view.layoutSystem.SetLayoutDirection(element, direction);
            }
        }

        public LayoutFlowType GetLayoutFlow(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutFlow;
        }

        public void SetLayoutFlow(LayoutFlowType flowType, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutFlow = flowType;
            view.layoutSystem.SetInFlow(element, flowType != LayoutFlowType.OutOfFlow);
        }

        public LayoutType GetLayoutType(StyleState state = StyleState.Normal) {
            return GetOrCreateStyle(state).layoutType;
        }

        public void SetLayoutType(LayoutType layout, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutType = layout;
            view.layoutSystem.SetLayoutType(element, layout);
        }

        public LayoutWrap GetLayoutWrap(StyleState state = StyleState.Normal) {
            return GetStyle(state).layoutWrap;
        }

        public void SetLayoutWrap(LayoutWrap layoutWrap, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).layoutWrap = layoutWrap;
            view.layoutSystem.SetLayoutWrap(element, layoutWrap);
        }

        public MainAxisAlignment GetMainAxisAlignment(StyleState state = StyleState.Normal) {
            return GetStyle(state).mainAxisAlignment;
        }

        public void SetMainAxisAlignment(MainAxisAlignment alignment, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).mainAxisAlignment = alignment;
            view.layoutSystem.SetMainAxisAlignment(element, alignment);
        }

        public CrossAxisAlignment GetCrossAxisAlignment(StyleState state = StyleState.Normal) {
            return GetStyle(state).crossAxisAlignment;
        }

        public void SetCrossAxisAlignment(CrossAxisAlignment alignment, StyleState state = StyleState.Normal) {
            GetOrCreateStyle(state).crossAxisAlignment = alignment;
            view.layoutSystem.SetCrossAxisAlignment(element, alignment);
        }

    }

}