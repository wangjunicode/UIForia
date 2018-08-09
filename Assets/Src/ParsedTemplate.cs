using System;
using System.Collections.Generic;
using Rendering;

namespace Src {

    public class ParsedTemplate {

        public Type type;
        public string filePath;
        public UIElementTemplate rootElement;
        public List<StyleTemplate> styles;
        public List<ImportDeclaration> imports;
        public List<ContextDefinition> contexts;
        public Dictionary<string, UIElement> slotMap;

        public void Hydrate(UIElement target, TemplateScope scope) {
            
        }

        public bool CompileAndTypeCheck() {

            rootElement.TypeCheck();
            
            return true;
        }

        public UIElement Instantiate(UIView view, List<object> rootProps, Dictionary<string, UIElement> inputSlotMap) {
    
            

            return null;
        }

        private StyleTemplate GetStyleTemplate(string id) {
            for (int i = 0; i < styles.Count; i++) {
                if (styles[i].id == id) return styles[i];
            }

            return null;
        }
    }
    
}
