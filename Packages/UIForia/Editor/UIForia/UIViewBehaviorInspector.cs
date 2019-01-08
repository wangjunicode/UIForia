using System;
using System.Collections.Generic;
using UnityEditor;

namespace UIForia.Editor {

    [CustomEditor(typeof(UIViewBehavior))]
    public class UIViewBehaviorInspector : UnityEditor.Editor {

        private Type[] types;
        private string[] names;
        
        public void OnEnable() {
            TypeProcessor.TypeData[] typeData = TypeProcessor.GetTemplateTypes();

            List<Type> validTypes = new List<Type>();
            for (int i = 0; i < typeData.Length; i++) {
                if (typeData[i].type.Assembly.FullName.StartsWith("UIForia")) {
                    continue;
                }
                validTypes.Add(typeData[i].type);
            }
            
            types = new Type[validTypes.Count];
            names = new string[types.Length];
            for (int i = 0; i < types.Length; i++) {
                types[i] = validTypes[i];
                names[i] = validTypes[i].FullName;
            }
        }
        
        public override void OnInspectorGUI() {
            UIViewBehavior behavior = (UIViewBehavior) target;
            if (behavior.type == null && behavior.typeName != null) {
                behavior.type = Type.GetType(behavior.typeName);
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Root Template");
            int index = Array.IndexOf(types, behavior.type);
            int newIndex = EditorGUILayout.Popup(index, names);
            EditorGUILayout.EndHorizontal();

            if (index != newIndex) {
                behavior.type = types[newIndex];
                behavior.typeName = behavior.type.AssemblyQualifiedName;
            }

            EditorGUILayout.ObjectField(serializedObject.FindProperty("camera"));
            serializedObject.ApplyModifiedProperties();
        }

    }

}