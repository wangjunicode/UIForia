using System.Collections.Generic;
using UIForia.Rendering;

namespace UIForia.Compilers.Style {
    public class UIStyleGroupContainer {
        public string name;
        public StyleType styleType;
        public IReadOnlyList<UIStyleGroup> groups;

        public UIStyleGroupContainer(string name, StyleType styleType, IReadOnlyList<UIStyleGroup> groups) {
            this.name = name;
            this.styleType = styleType;
            this.groups = groups;
        }
    }
}
