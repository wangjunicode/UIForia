using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Compilers.Style {
    
    public class StyleSheet {

        internal LightList<UIStyleGroupContainer> styleGroupContainers; 

        internal LightList<StyleConstant> constants;

        internal StyleSheet(LightList<StyleConstant> constants, LightList<UIStyleGroupContainer> styleGroupContainers) {
            this.constants = constants;
            this.styleGroupContainers = styleGroupContainers;
        }

        public UIStyleGroupContainer GetStyleGroupsByTagName(string tagName) {

            for (int i = 0; i < styleGroupContainers.Count; i++) {
                UIStyleGroupContainer container = styleGroupContainers[i];
                if (container.styleType == StyleType.Implicit && container.name == tagName) {
                    return container;
                }
            }

            return null;
        }

        public UIStyleGroupContainer GetStyleGroupByStyleName(string styleName) {
            for (int i = 0; i < styleGroupContainers.Count; i++) {
                UIStyleGroupContainer container = styleGroupContainers[i];
                if (container.styleType == StyleType.Shared && container.name == styleName) {
                    return container;
                }
            }
    
            return null;
        }
    }
}
