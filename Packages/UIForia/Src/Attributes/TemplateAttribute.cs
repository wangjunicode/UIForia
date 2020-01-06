using System;

namespace UIForia.Attributes {

    public enum TemplateType {

        Internal,
        String,
        File,
        DefaultFile

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TemplateAttribute : Attribute {

        public string source;
        public string filePath;
        public string templateId;
        public readonly TemplateType templateType;
        public string fullPathId;

        public TemplateAttribute() {
            this.templateType = TemplateType.DefaultFile;
            this.templateId = null;
            this.source = string.Empty;
            this.fullPathId = null;
        }

        public TemplateAttribute(TemplateType templateType, string sourceOrPath) {
            this.templateType = templateType;
            this.templateId = null;
            this.source = string.Empty; // set later 
            this.fullPathId = null;

            switch (templateType) {
                case TemplateType.DefaultFile:
                    break;
                case TemplateType.String:
                    this.filePath = "FILE";
                    this.source = sourceOrPath;
                    break;
                case TemplateType.File:
                case TemplateType.Internal:
                    this.fullPathId = sourceOrPath;
                    int idx = sourceOrPath.IndexOf('#');
                    if (idx < 0) {
                        this.filePath = sourceOrPath;
                    }
                    else {
                        this.templateId = sourceOrPath.Substring(idx + 1);
                        this.filePath = sourceOrPath.Substring(0, idx);
                    }

                    break;
            }
        }

        public TemplateAttribute(string source) : this(TemplateType.File, source) { }

    }

}