#if UNITY_EDITOR
using UnityEditor;

namespace UIForia.Editor {

    [CustomEditor(typeof(UIForiaSettings))]
    public class UIForiaSettingsEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            float dpi = EditorPrefs.GetFloat("UIFORIA_DPI_OVERRIDE", Application.originalDpiScaleFactor);
            dpi = EditorGUILayout.FloatField("DPI Override", dpi);
            if (dpi <= 0) {
                dpi = Application.originalDpiScaleFactor;
            }

            EditorPrefs.SetFloat("UIFORIA_DPI_OVERRIDE", dpi);
        }

    }

}
#endif