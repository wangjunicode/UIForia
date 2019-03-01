using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UIForia.Attributes;

namespace UIForia.Parsing.Expression {

    [DebuggerDisplay("{rawType.Name}")]
    public struct ProcessedType {

        public readonly Type rawType;
        private readonly TemplateAttribute templateAttr;
        
        public ProcessedType(Type rawType) {
            this.rawType = rawType;
            templateAttr = rawType.GetCustomAttribute<TemplateAttribute>();
        }

        public string GetTemplate() {
            if (templateAttr == null) {
                throw new Exception($"Template not defined for {rawType.Name}");
            }

            if (templateAttr.templateType == TemplateType.File) {
                return TryReadFile(UnityEngine.Application.dataPath + "/" + templateAttr.template);
            }

            return templateAttr.template;
        }

        private static string TryReadFile(string path) {
            if (!path.EndsWith(".xml")) {
                path += ".xml";
            }
            
            // todo should probably be cached, but be careful about reloading

            try {
                return File.ReadAllText(path);
            }
            catch (FileNotFoundException e) {
                throw e;
            }
            catch (Exception e) {
                return null;
            }
        }

        public bool HasTemplatePath() {
            return templateAttr.templateType == TemplateType.File;
        }

        // path from Assets directory
        public string GetTemplatePath() {
            return !HasTemplatePath() ? rawType.AssemblyQualifiedName : templateAttr.template;
        }

        public string GetTemplateName() {
            if (templateAttr == null) {
                return "Null";
            }

            if (templateAttr.templateType == TemplateType.File) {
                return Path.GetFileName(templateAttr.template);
            }

            return "InlineTemplate: " + rawType.Name;
        }

    }

}