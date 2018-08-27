using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Src {

    [DebuggerDisplay("{rawType.Name}")]
    public class ProcessedType {

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
                return File.ReadAllText(Application.dataPath + "/" + templateAttr.template);
            }

            return templateAttr.template;
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