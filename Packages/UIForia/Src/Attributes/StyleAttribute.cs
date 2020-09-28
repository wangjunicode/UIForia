using System;
using System.Runtime.CompilerServices;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class StyleAttribute : Attribute {

        public readonly string styleNames;
        public readonly string filePath;

#if UNITY_EDITOR
        public StyleAttribute(string styleNames, [CallerFilePath] string DO_NOT_USE = "") {
            this.styleNames = styleNames;
            this.filePath = DO_NOT_USE;
        }
#else
        public StyleAttribute(string styleNames, string DO_NOT_USE = "") {
            this.styleNames = styleNames;
            this.filePath = DO_NOT_USE;
        }
#endif

    }

}