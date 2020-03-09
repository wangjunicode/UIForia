using System;
using System.Runtime.CompilerServices;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TemplateAttribute : Attribute {

        public string source;
        public string filePath;
        public string templateId;
        public string templatePath;
        public string elementPath;

        public TemplateAttribute(string sourceOrPath = "", [CallerFilePath] string DO_NOT_USE_ALSO = "") {
            this.templateId = null;
            this.source = string.Empty;
            this.templatePath = null;
            this.elementPath = DO_NOT_USE_ALSO;
            this.templatePath = sourceOrPath;
            int idx = sourceOrPath.IndexOf('#');
            if (idx < 0) {
                this.filePath = sourceOrPath;
            }
            else {
                this.templateId = sourceOrPath.Substring(idx + 1);
                this.filePath = sourceOrPath.Substring(0, idx);
            }
        }

        // public TemplateAttribute(TemplateType templateType, string sourceOrPath, [CallerFilePath] string elementPath = "") {
        //     this.templateType = templateType;
        //     this.templateId = null;
        //     this.source = string.Empty; // set later 
        //     this.fullPathId = null;
        //     this.elementPath = elementPath;
        //
        //     switch (templateType) {
        //         case TemplateType.DefaultFile:
        //             break;
        //         case TemplateType.File:
        //         case TemplateType.Internal:
        //             this.fullPathId = sourceOrPath;
        //             int idx = sourceOrPath.IndexOf('#');
        //             if (idx < 0) {
        //                 this.filePath = sourceOrPath;
        //             }
        //             else {
        //                 this.templateId = sourceOrPath.Substring(idx + 1);
        //                 this.filePath = sourceOrPath.Substring(0, idx);
        //             }
        //
        //             break;
        //     }
        // }

        // ReSharper disable once ExplicitCallerInfoArgument
        // public TemplateAttribute(string source, [CallerFilePath] string DO_NOT_USE = "") : this(0, source, DO_NOT_USE) { }

    }

}