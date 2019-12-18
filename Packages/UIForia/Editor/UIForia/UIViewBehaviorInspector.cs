using System;
using System.Collections.Generic;
using System.IO;
using UIForia.Compilers;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UIForia.Editor {

    [CustomEditor(typeof(UIViewBehavior))]
    public class UIViewBehaviorInspector : UnityEditor.Editor {

        private Type[] types;
        private string[] names;
        private bool didEnable;
        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() {
            // todo try to track if pre-compiled templates are out of date or not
        }
        
        public void OnEnable() {
            didEnable = true;
            LightList<ProcessedType> typeData = TypeProcessor.GetTemplateTypes();

            List<Type> validTypes = new List<Type>();
            for (int i = 0; i < typeData.size; i++) {
                if (typeData[i].rawType.Assembly.FullName.StartsWith("UIForia")) {
                    continue;
                }
                validTypes.Add(typeData[i].rawType);
            }
            
            types = new Type[validTypes.Count];
            names = new string[types.Length];
            for (int i = 0; i < types.Length; i++) {
                types[i] = validTypes[i];
                names[i] = validTypes[i].FullName;
            }
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            UIViewBehavior behavior = (UIViewBehavior) target;
            string typeName = serializedObject.FindProperty("typeName").stringValue;
            if (behavior.type != null && typeName != behavior.type.AssemblyQualifiedName) {
                behavior.type = Type.GetType(typeName);
            }
            else if (behavior.type == null) {
                behavior.type = Type.GetType(typeName);
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Root Template");

            if (types == null || types.Length == 0) {
                if(!didEnable) OnEnable();
                EditorGUILayout.EndHorizontal();
                return;
            }
            
            if (behavior.type == null) {
                behavior.type = types[0];
            }

            int index = Array.IndexOf(types, behavior.type);
            int newIndex = EditorGUILayout.Popup(index, names);
            EditorGUILayout.EndHorizontal();

            if (index != newIndex) {
                behavior.type = types[newIndex];
                behavior.typeName = behavior.type.AssemblyQualifiedName;
                EditorSceneManager.MarkSceneDirty(behavior.gameObject.scene);
            }

            behavior.usePreCompiledTemplates = GUILayout.Toggle(behavior.usePreCompiledTemplates, "Use Precompiled");
            
            if (GUILayout.Button("Generate Code")) {
                
                TemplateCodeGenerator.Generate(behavior.type, behavior.GetTemplateSettings());
                
//                TemplateCompiler compiler = new TemplateCompiler(settings);
//                
//                // maybe this should also know the root type for an application
//                PreCompiledTemplateData compiledOutput = new PreCompiledTemplateData(settings);
//
//                compiler.CompileTemplates(behavior.type, compiledOutput);
//
//                compiledOutput.GenerateCode();

            }
            
            EditorGUILayout.ObjectField(serializedObject.FindProperty("camera"));
            serializedObject.FindProperty("typeName").stringValue = behavior.typeName;
            serializedObject.ApplyModifiedProperties();
        }

    }

}