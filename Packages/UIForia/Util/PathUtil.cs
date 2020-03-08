using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace UIForia.Util {

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RecordFilePathAttribute : Attribute {

        public readonly string filePath;

        public RecordFilePathAttribute([CallerFilePath] string filePath = "") {
            this.filePath = filePath;
        }

    }

    public static class PathUtil {

        public static string GetCallerFilePath([CallerFilePath] string sourceFilePath = "") {
            return sourceFilePath;
        }

        public static string GetCallerDirectoryPath([CallerFilePath] string sourceFilePath = "") {
            // might need to ensure ends with \
            return Path.GetDirectoryName(sourceFilePath);
        }
        
    }

}