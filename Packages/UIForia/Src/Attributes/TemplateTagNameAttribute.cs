using System;

namespace UIForia.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    public class TemplateTagNameAttribute : Attribute {

        public readonly string tagName;

        public TemplateTagNameAttribute(string tagName) {
            this.tagName = tagName;
        }

    }
}