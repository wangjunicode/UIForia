using System.IO;

namespace UIForia.Style {

    public static class PathUtil {

        private static readonly string s_AppBase = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
        private static readonly string s_TempBase = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "Temp"));
        
        public static string GetTempFilePath(string fileName) {
            return Path.GetFullPath(Path.Combine(s_TempBase, fileName));
        }

        public static string ProjectRelativePath(string path) {
            return path.Substring(s_AppBase.Length + 1, path.Length - s_AppBase.Length - 1);
        }

    }

}