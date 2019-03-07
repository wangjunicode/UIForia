using System.Collections.Generic;
using UIForia.Rendering;

namespace UIForia.Compilers.Style {

    // can this become a struct?
    public class UIStyleGroupContainer {

        public string name;
        public StyleType styleType;
        public IReadOnlyList<UIStyleGroup> groups; // can this become an int[]?

        public readonly bool hasAttributeStyles;
        
        public UIStyleGroupContainer(string name, StyleType styleType, IReadOnlyList<UIStyleGroup> groups) {
            this.name = name;
            this.styleType = styleType;
            this.groups = groups;
            for (int i = 0; i < groups.Count; i++) {
                if (groups[i].HasAttributeRule) {
                    hasAttributeStyles = true;
                    break;
                }
            }
        }

    }

}