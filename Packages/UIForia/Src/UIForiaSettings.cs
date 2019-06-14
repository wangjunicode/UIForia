using System.IO;
using UnityEngine;

namespace UIForia {

    public class UIForiaSettings : ScriptableObject {

        public bool loadTemplatesFromStreamingAssets = false;
        public Material svgxMaterial;
        
        internal static string s_InternalStreamingPath;
        internal static string s_InternalNonStreamingPath;
        internal static string s_UserNonStreamingPath;
        internal static string s_UserStreamingPath;

        public void OnEnable() {
            s_InternalStreamingPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForiaInternal");
            s_InternalNonStreamingPath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Src");
            s_UserNonStreamingPath = UnityEngine.Application.dataPath;
            s_UserStreamingPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForia");
        }

        public string GetInternalTemplatePath(string fileName) {
            if (loadTemplatesFromStreamingAssets) {
                return Path.GetFullPath(Path.Combine(s_InternalStreamingPath, fileName));
            }
            else {
                return Path.GetFullPath(Path.Combine(s_InternalNonStreamingPath, fileName));
            }
        }
        
        public string GetTemplatePath(string fileName) {
            if (loadTemplatesFromStreamingAssets) {
                return Path.GetFullPath(Path.Combine(s_UserStreamingPath, fileName));
            }
            else {
                return Path.GetFullPath(Path.Combine(s_UserNonStreamingPath, fileName));
            }
        }

        public string GetStylePath(string fileName) {
            if (loadTemplatesFromStreamingAssets) {
                return Path.GetFullPath(Path.Combine(s_UserStreamingPath, fileName));
            }
            else {
                return Path.GetFullPath(Path.Combine(s_UserNonStreamingPath, fileName));
            }
        }
    }

}