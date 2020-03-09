using System;
using System.Runtime.CompilerServices;

namespace UIForia.Util {

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RecordFilePathAttribute : Attribute {

        public readonly string filePath;

#if UNITY_EDITOR
        public RecordFilePathAttribute([CallerFilePath] string filePath = "") {
            this.filePath = filePath;
        }
#else
        public RecordFilePathAttribute(string filePath = "") {}
#endif

    }

}