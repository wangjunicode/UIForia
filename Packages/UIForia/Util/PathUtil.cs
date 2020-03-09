using System.IO;
using System.Runtime.CompilerServices;

namespace UIForia.Util {

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