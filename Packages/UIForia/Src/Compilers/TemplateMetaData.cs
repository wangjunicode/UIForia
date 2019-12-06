using System;
using System.Collections.Generic;
using UIForia.Compilers.Style;

namespace UIForia.Compilers {

    public struct StyleSheetReference {

        public string alias;
        public StyleSheet styleSheet;

        public StyleSheetReference(string alias, StyleSheet styleSheet) {
            this.alias = alias;
            this.styleSheet = styleSheet;
        }

    }
    
    public class TemplateMetaData {

        public int id;
        public string filePath;
        public int usages;
        public StyleSheetReference[] styleReferences; // dictionary? sorted by name? trie?

        public CompiledTemplateData compiledTemplateData;
        internal UIStyleGroupContainer[] styleMap;
        
        public TemplateMetaData(int id, string filePath, UIStyleGroupContainer[] styleMap, StyleSheetReference[] styleReferences) {
            this.id = id;
            this.filePath = filePath;
            this.styleReferences = styleReferences;
            this.styleMap = styleMap;
        }
        
        public UIStyleGroupContainer ResolveStyleByName(string name) {

            if (name.Contains(".")) {
                throw new NotImplementedException("Cannot resolve style name with aliases yet");    
            }
            
            for (int i = 0; i < styleReferences.Length; i++) {
                if (styleReferences[i].alias == name) {
                    return styleReferences[i].styleSheet.GetStyleByName(name);
                }

            }
            
            return null;
        }

        public UIStyleGroupContainer GetStyleById(int styleId) {
            return styleMap[styleId];
        }

    }

}