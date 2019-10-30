using System.Collections.Generic;
using UIForia.Compilers.Style;
using UIForia.Rendering;

namespace UIForia.Compilers {

    public class StyleRegistry { }

    public class TemplateMetaData {

        public int id;
        public string filePath;
        public int usages;
        public UIStyle[] styleReferences; // dictionary? sorted by name? trie?

        public UIStyleGroupContainer implicitRootStyle;
        public Dictionary<string, UIStyleGroupContainer> implicitStyleMap;
        
        public TemplateMetaData(int id, string filePath, UIStyle[] styleReferences) {
            this.id = id;
            this.filePath = filePath;
            this.styleReferences = styleReferences;
        }
        
        public UIStyle GetStyleByName(string name) {
            if (styleReferences == null) return default;
            
            for (int i = 0; i < styleReferences.Length; i++) {
                
                if (id == i) {
                    
                }
                
            }
            
            return default;
        }

        public UIStyle GetStyleById(int id) {
            return default;
        }

        public void CompileStyles(StyleRegistry registry) {
            
        }

    }

}