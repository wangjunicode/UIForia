using Rendering;
using UnityEditor;
using UnityEditor.UI;

namespace Src.Editor {

    [CustomEditor(typeof(BorderedImage))]
    public class BorderedImageInspector : ImageEditor {

        private SerializedProperty borderColor;
        private SerializedProperty borderSizes;
        
        protected override void OnEnable() {
            base.OnEnable();
            borderColor = serializedObject.FindProperty("borderColor");
            borderSizes = serializedObject.FindProperty("border");
        }

        public override void OnInspectorGUI() {
            EditorGUILayout.PropertyField (m_Color);
            EditorGUILayout.PropertyField (borderColor);
            EditorGUILayout.PropertyField(borderSizes);
            serializedObject.ApplyModifiedProperties ();
        }

    }

}