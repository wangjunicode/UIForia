using UIForia;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildTest : IPreprocessBuildWithReport {

    public int callbackOrder => 0;
    
    public void OnPreprocessBuild(BuildReport report) {
        object o = AssetDatabase.FindAssets("UIForia Settings");
        UIForiaSettings a = (UIForiaSettings) o;
        a.loadTemplatesFromStreamingAssets = true;
        Builder.BuildTemplates();
        AssetDatabase.SaveAssets();
    }

}