using System.Collections.Generic;
using UIForia.Rendering;
using UnityEditor;

namespace UIForia.Compilers.Style {

    // todo -- conver to struct
    public class UIStyleGroupContainer {

        public int id;
        public string name;
        public StyleType styleType;
        public IReadOnlyList<UIStyleGroup> groups; // can this become an int[]?
        public readonly bool hasAttributeStyles;
        public GUID guid;
        
        public UIStyleGroupContainer(int id, string name, StyleType styleType, IReadOnlyList<UIStyleGroup> groups) {
            this.id = id;
            this.name = name;
            this.styleType = styleType;
            this.groups = groups;
            this.guid = GUID.Generate();
            for (int i = 0; i < groups.Count; i++) {
                if (groups[i].HasAttributeRule) {
                    hasAttributeStyles = true;
                    break;
                }
            }
        }

    }

}