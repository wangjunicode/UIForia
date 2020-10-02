using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Parsing;

namespace UIForia {
    internal class UIModuleDefinition {

        public string Prototype;
        public string TemplateLocator;
        public string StyleLocator;
        public string AssetLocator;
        public string[] DefaultNamespaces;
        public string[] ImplicitStyleReferences;
        public string[] ImplicitModuleReferences;

    }

    internal class UIModule {

        public string location;
        public UIModuleDefinition data;

        public Func<TemplateLookup, TemplateLocation> templateLocator;
        public Func<string, string, string> styleLocator;
        public Dictionary<string, ProcessedType> tagNameMap;
        public string path;
        public string name;
        public UIModule[] implicitModuleReferences;

        public UIModule() {
            tagNameMap = new Dictionary<string, ProcessedType>();
        }

        public void Initialize() {
            if (!string.IsNullOrEmpty(data.TemplateLocator)) {
                if (TypeScanner.templateLocators.TryGetValue(data.TemplateLocator, out Func<TemplateLookup, TemplateLocation> locator)) {
                    this.templateLocator = locator;
                }
            }

            if (!string.IsNullOrEmpty(data.StyleLocator)) {
                if (TypeScanner.styleLocators.TryGetValue(data.StyleLocator, out Func<string, string, string> locator)) {
                    this.styleLocator = locator;
                }
            }
        }

        public TemplateLocation ResolveTemplate(TemplateLookup lookup) {
            return templateLocator?.Invoke(lookup) ?? DefaultLocators.LocateTemplate(lookup);
        }

        public string ResolveStyle(string elementPath, string stylePath) {
            return styleLocator?.Invoke(elementPath, stylePath) ?? DefaultLocators.LocateStyle(elementPath, stylePath);
        }

    }

}