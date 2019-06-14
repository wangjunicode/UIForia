using System.IO;
using UnityEditor;
using UnityEngine;

namespace UIForia {

    public static class Builder {

        [MenuItem("UIForia/Create Options Object")]
        public static void CreateOptionsObject() {
         
            UIForiaSettings asset = ScriptableObject.CreateInstance<UIForiaSettings>();
            Shader shader = Shader.Find("UIForia/BatchedTransparent");
            Material svgxMaterial = new Material(shader);
            
            AssetDatabase.CreateAsset(svgxMaterial, "Assets/Resources/UIForiaMaterial.mat");
            asset.svgxMaterial = svgxMaterial;
            AssetDatabase.CreateAsset(asset, "Assets/Resources/UIForiaSettings.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        [MenuItem("UIForia/Build Templates")]
        public static void BuildTemplates() {
            string userPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForia");
            string internalDestPath = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "UIForiaInternal");
            string internalSourcePath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Src");

            if (Directory.Exists(userPath)) {
                Directory.Delete(userPath, true);
            }

            if (Directory.Exists(internalDestPath)) {
                Directory.Delete(internalDestPath, true);
            }

            string[] files = Directory.GetFiles(UnityEngine.Application.dataPath, "*.xml", SearchOption.AllDirectories);

            foreach (string file in files) {
                string newPath = file.Replace(UnityEngine.Application.dataPath, userPath);
                Directory.CreateDirectory(new FileInfo(newPath).Directory.FullName);
                File.Copy(file, newPath, true);
            }

            files = Directory.GetFiles(UnityEngine.Application.dataPath, "*.style", SearchOption.AllDirectories);
            
            foreach (string file in files) {
                string newPath = file.Replace(UnityEngine.Application.dataPath, userPath);
                Directory.CreateDirectory(new FileInfo(newPath).Directory.FullName);
                File.Copy(file, newPath, true);
            }
            
            files = Directory.GetFiles(internalSourcePath, "*.xml", SearchOption.AllDirectories);

            foreach (string file in files) {
                string newPath = file.Replace(internalSourcePath, internalDestPath);
                Directory.CreateDirectory(new FileInfo(newPath).Directory.FullName);
                File.Copy(file, newPath, true);
            }

            AssetDatabase.Refresh();
        }

    }

}