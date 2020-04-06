using System;
using UIForia.Templates;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Parsing {

    public class TemplateShell {

        public readonly Module module;
        public readonly string filePath;
        public readonly StructList<UsingDeclaration> usings;
        public readonly StructList<StyleDefinition> styles;
        public readonly LightList<string> referencedNamespaces;
        public SizedArray<TemplateRootNode> templateRootNodes;
        
        internal DateTime lastParseVersion;

        public TemplateShell(Module module, string filePath) {
            this.module = module;
            this.filePath = filePath;
            this.usings = new StructList<UsingDeclaration>(2);
            this.styles = new StructList<StyleDefinition>(2);
            this.referencedNamespaces = new LightList<string>(4);
        }
        
        public bool ReportError(TemplateLineInfo lineInfo, string error) {
            // todo -- diagnostics
            Debug.LogError(filePath + "line " + lineInfo + ": " + error);
            return false;
        }
        
        public TemplateRootNode GetTemplateRoot(string templateId) {
            
            if (templateRootNodes.array == null) return null;
            
            for (int i = 0; i < templateRootNodes.size; i++) {
                if (templateRootNodes.array[i].templateName == templateId) {
                    return templateRootNodes.array[i];
                }
            }

            return null;
        }

        public void Reset() {
            usings.Clear();
            styles.Clear();
            referencedNamespaces.Clear();
            templateRootNodes.Clear();
        }

    }

}