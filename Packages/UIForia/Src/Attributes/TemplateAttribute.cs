using System;
using System.Runtime.CompilerServices;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TemplateAttribute : Attribute {

        public readonly string templateId;
        public readonly string templatePath;
        public readonly string elementPath;

        public TemplateAttribute(string templatePath = "", [CallerFilePath] string DO_NOT_USE = "") {
            this.templateId = null;
            this.elementPath = DO_NOT_USE;
            int idx = templatePath.IndexOf('#');
            if (idx < 0) {
                this.templatePath = templatePath;
            }
            else {
                this.templateId = templatePath.Substring(idx + 1);
                this.templatePath = templatePath.Substring(0, idx);
            }
        }

        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        public sealed class FromString : Attribute {

            public readonly string source;
            public readonly string elementPath;

            public FromString(string source, [CallerFilePath] string DO_NOT_USE = "") {
                this.source = source;
                this.elementPath = DO_NOT_USE;
            }

        }

    }

    // public enum TemplateType {
    //
    //     Internal,
    //     File,
    //     DefaultFile
    //
    // }
    //
    // [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    // public sealed class TemplateAttribute : Attribute {
    //
    //     public string source;
    //     public string filePath;
    //     public string templateId;
    //     public readonly TemplateType templateType;
    //     public string fullPathId;
    //
    //     public TemplateAttribute() {
    //         this.templateType = TemplateType.DefaultFile;
    //         this.templateId = null;
    //         this.source = string.Empty;
    //         this.fullPathId = null;
    //     }
    //
    //     public TemplateAttribute(TemplateType templateType, string sourceOrPath) {
    //         this.templateType = templateType;
    //         this.templateId = null;
    //         this.source = string.Empty; // set later 
    //         this.fullPathId = null;
    //
    //         switch (templateType) {
    //             case TemplateType.DefaultFile:
    //                 break;
    //             case TemplateType.File:
    //             case TemplateType.Internal:
    //                 this.fullPathId = sourceOrPath;
    //                 int idx = sourceOrPath.IndexOf('#');
    //                 if (idx < 0) {
    //                     this.filePath = sourceOrPath;
    //                 }
    //                 else {
    //                     this.templateId = sourceOrPath.Substring(idx + 1);
    //                     this.filePath = sourceOrPath.Substring(0, idx);
    //                 }
    //
    //                 break;
    //         }
    //     }
    //
    //     public TemplateAttribute(string source) : this(TemplateType.File, source) { }
    //
    // }

}