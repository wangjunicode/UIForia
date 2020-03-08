using System;
using System.IO;
using System.Linq;
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

        public static Uri Parent(this Uri uri) {
            return new Uri(uri.AbsoluteUri.Remove(uri.AbsoluteUri.Length - uri.Segments.Last().Length - uri.Query.Length).TrimEnd('/'));
        }
        
        public static string GetCallerFilePath([CallerFilePath] string sourceFilePath = "") {
            return sourceFilePath;
        }

        public static string GetCallerDirectoryPath([CallerFilePath] string sourceFilePath = "") {
            // might need to ensure ends with \
            return Path.GetDirectoryName(sourceFilePath);
        }
        
    }

}