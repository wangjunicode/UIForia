using UIForia.Rendering;

namespace UIForia.Compilers.Style {
    
    public class StyleSheet {

        internal UIStyleGroupContainer[] styleGroupContainers; 

        internal StyleConstant[] constants;

        internal StyleSheet(StyleConstant[] constants, UIStyleGroupContainer[] styleGroupContainers) {
            this.constants = constants;
            this.styleGroupContainers = styleGroupContainers;
        }

        public UIStyleGroupContainer GetStyleGroupsByTagName(string tagName) {

            for (int i = 0; i < styleGroupContainers.Length; i++) {
                UIStyleGroupContainer container = styleGroupContainers[i];
                if (container.styleType == StyleType.Implicit && container.name == tagName) {
                    return container;
                }
            }

            return null;
        }

        public UIStyleGroupContainer GetStyleGroupByStyleName(string styleName) {
            for (int i = 0; i < styleGroupContainers.Length; i++) {
                UIStyleGroupContainer container = styleGroupContainers[i];
                if (container.styleType == StyleType.Shared && container.name == styleName) {
                    return container;
                }
            }
    
            return null;
        }
    }
}
