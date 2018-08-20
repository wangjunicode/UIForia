using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Src {

    [DebuggerDisplay("{rawType.Name}")]
    public class ProcessedType {

        public readonly Type rawType;
        public readonly List<FieldInfo> propFields;
        public readonly List<FieldInfo> contextProperties;
        
        private string templatePath;

        private static object[] constructorParameters = new object[2];

        public ProcessedType(Type rawType) {
            this.rawType = rawType;
            propFields = new List<FieldInfo>();
            contextProperties = new List<FieldInfo>();
        }

        public string GetTemplatePath() {
            if (templatePath != null) return templatePath;
            TemplateAttribute attr = rawType.GetCustomAttribute<TemplateAttribute>();
            if (attr != null) {
                templatePath = attr.template;
            }
            return templatePath;
        }

        public bool HasProp(string propName) {

            for (int i = 0; i < propFields.Count; i++) {
                if (propFields[i].Name == propName) {
                    return true;
                }
            }

            return false;
        }

        public FieldInfo GetField(string bindingKey) {
            for (int i = 0; i < propFields.Count; i++) {
                if (propFields[i].Name == bindingKey) return propFields[i];
            }
            return null;
        }

    }

}