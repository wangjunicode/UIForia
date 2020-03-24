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