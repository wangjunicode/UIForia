using System;

namespace Src {

    public enum TemplateType {

        String,
        File

    }
    public class TemplateAttribute : Attribute {
        public readonly string template;
        public readonly TemplateType templateType;
        
        public TemplateAttribute(TemplateType templateType, string template) {
            this.templateType = templateType;
            this.template = template;
        }
        
        public TemplateAttribute(string template) {
            this.templateType = TemplateType.File;
            this.template = "/" + template;
        }
        
    }
}