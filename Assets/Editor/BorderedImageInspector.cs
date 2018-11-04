using Src.Rendering;
using UnityEditor;
using UnityEditor.UI;

namespace Src.Editor {

    [CustomEditor(typeof(BorderedImage))]
    public class BorderedImageInspector : ImageEditor {

        private SerializedProperty borderColor;
        private SerializedProperty borderSizes;
        private SerializedProperty borderRadii;
        
        protected override void OnEnable() {
            base.OnEnable();
            borderColor = serializedObject.FindProperty("borderColor");
            borderRadii = serializedObject.FindProperty("borderRadii");
            borderSizes = serializedObject.FindProperty("border");
        }

        public override void OnInspectorGUI() {
            EditorGUILayout.PropertyField (m_Color);
            EditorGUILayout.PropertyField (borderColor);
            EditorGUILayout.PropertyField(borderSizes);
            EditorGUILayout.PropertyField(borderRadii);
            serializedObject.ApplyModifiedProperties();
        }

    }

}