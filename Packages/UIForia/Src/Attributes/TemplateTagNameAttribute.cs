using System;
using System.Runtime.CompilerServices;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TemplateTagNameAttribute : Attribute {

        public readonly string tagName;
        public readonly string filePath;

#if UNITY_EDITOR
        public TemplateTagNameAttribute(string tagName, [CallerFilePath] string DO_NOT_USE = "") {
            this.tagName = tagName;
            this.filePath = DO_NOT_USE;
        }
#else
        public TemplateTagNameAttribute(string tagName, string DO_NOT_USE = "") {
            this.tagName = tagName;
            this.filePath = DO_NOT_USE;
        }
#endif

    }

}