using System.Collections.Generic;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Compilers.Style {
    
    public class StyleSheet {

        internal LightList<StyleConstant> constants;

        internal LightList<UIStyleGroup> styleGroups;

        internal StyleSheet(LightList<StyleConstant> constants, LightList<UIStyleGroup> styleGroups) {
            this.constants = constants;
            this.styleGroups = styleGroups;
        }

        public IList<UIStyleGroup> GetStyleGroupsByElement(UIElement element) {
            for (int index = 0; index < styleGroups.Count; index++) {
                UIStyleGroup styleGroup = styleGroups[index];
                
            }

            return null;
        }
    }
}
