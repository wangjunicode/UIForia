using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Src {

    [DebuggerDisplay("{type.Name}")]
    public class ProcessedType {

        public readonly Type type;
        public readonly List<FieldInfo> propFields;
        public readonly List<FieldInfo> contextProperties;
        public readonly bool isPrimitive;
        
        private string templatePath;

        private static object[] constructorParameters = new object[2];

        public ProcessedType(Type type) {
            this.type = type;
            propFields = new List<FieldInfo>();
            contextProperties = new List<FieldInfo>();
            isPrimitive = type.IsSubclassOf(typeof(UIElementPrimitive));
        }

        public string GetTemplatePath() {
            if (templatePath != null) return templatePath;
            TemplateAttribute attr = type.GetCustomAttribute<TemplateAttribute>();
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