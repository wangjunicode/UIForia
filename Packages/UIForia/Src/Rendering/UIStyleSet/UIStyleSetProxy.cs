using UIForia.Util;

namespace UIForia.Rendering {

    public partial struct UIStyleSetProxy {

        private IntMap_Deprecated<StyleProperty> propertyMap;
        private StyleState state;

        internal UIStyleSetProxy(UIStyleSet styleSet) {
            this.propertyMap = styleSet.propertyMap;
            this.state = styleSet.currentState;
        }

        public bool IsHovered {
            get => (state & StyleState.Hover) != 0;
        }

        public bool IsActive {
            get => (state & StyleState.Active) != 0;
        }

        public bool IsFocused {
            get => (state & StyleState.Focused) != 0;
        }

    }

}