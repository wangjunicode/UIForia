using System.Collections.Generic;
using UIForia.Compilers.Style;

namespace UIForia.Compilers {

    public class TemplateMetaData {

        public int id;
        public string filePath;
        public int usages;
        public UIStyleGroupContainer[] styleReferences; // dictionary? sorted by name? trie?

        public UIStyleGroupContainer implicitRootStyle;
        public Dictionary<string, UIStyleGroupContainer> implicitStyleMap;
        
        public TemplateMetaData(int id, string filePath, UIStyleGroupContainer[] styleReferences) {
            this.id = id;
            this.filePath = filePath;
            this.styleReferences = styleReferences;
        }
        
        public UIStyleGroupContainer ResolveStyleByName(string name) {
            if (styleReferences == null) return default;
            
            for (int i = 0; i < styleReferences.Length; i++) {
                
            }
            
            return default;
        }

        public UIStyleGroupContainer GetStyleById(int id) {
            return default;
        }

    }

}