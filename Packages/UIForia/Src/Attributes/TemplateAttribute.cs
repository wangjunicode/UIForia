using System;

namespace UIForia.Attributes {

    public enum TemplateType {

        Internal,
        String,
        File

    }
    
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TemplateAttribute : Attribute {
        public readonly string template;
        public readonly TemplateType templateType;
        
        public TemplateAttribute(TemplateType templateType, string template) {
            this.templateType = templateType;
            this.template = template;
        }
        
        public TemplateAttribute(string template) {
            this.templateType = TemplateType.File;
            this.template = template;
        }
        
    }
}