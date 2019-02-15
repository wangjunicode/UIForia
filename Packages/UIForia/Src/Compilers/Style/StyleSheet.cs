using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Compilers.Style {
    
    public class StyleSheet {

        public LightList<StyleConstant> constants;

        public LightList<UIStyleGroup> styleGroups;

        public StyleSheet(LightList<StyleConstant> constants, LightList<UIStyleGroup> styleGroups) {
            this.constants = constants;
            this.styleGroups = styleGroups;
        }
    }
}
