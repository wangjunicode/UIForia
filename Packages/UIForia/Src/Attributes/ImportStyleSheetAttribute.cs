using System;
using System.Runtime.CompilerServices;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ImportStyleSheetAttribute : Attribute {

        public readonly string styleSheet;
        public readonly string filePath;

#if UNITY_EDITOR
        public ImportStyleSheetAttribute(string styleSheet, [CallerFilePath] string DO_NOT_USE = "") {
            this.styleSheet = styleSheet;
            this.filePath = DO_NOT_USE;
        }
#else
        public ImportStyleSheetAttribute(string styleSheet, string DO_NOT_USE = "") {
            this.styleSheet = styleSheet;
            this.filePath = DO_NOT_USE;
        }
#endif

    }

}