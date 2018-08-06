using System;

namespace Src {
    public class TemplateAttribute : Attribute {
        public readonly string template;

        public TemplateAttribute(string template) {
            this.template = "/" + template;
        }
    }
}