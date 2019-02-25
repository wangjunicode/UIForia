using System.Collections.Generic;
using UIForia.Rendering;

namespace UIForia.Compilers.Style {

    public class UIStyleGroupContainer {

        public string name;
        public StyleType styleType;
        public IReadOnlyList<UIStyleGroup> groups;

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