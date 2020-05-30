using System;
using System.Collections.Generic;
using System.Linq;
using UIForia.Elements;
using UIForia.Util;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UIForia {

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntryPointAttribute : Attribute { }

#if UNITY_EDITOR

    [CustomEditor(typeof(VertigoRuntimeLauncher))]
    public class VertigoRuntimeLauncherInspector : Editor {

        private List<Type> validEntryPoints;

        public void OnEnable() {

            TypeCache.TypeCollection entryPoints = TypeCache.GetTypesWithAttribute<EntryPointAttribute>();

            validEntryPoints = new List<Type>(entryPoints.Count);

            foreach (Type entryPoint in entryPoints) {
                if (entryPoint.IsAbstract) {
                    continue;
                }

                if (!entryPoint.IsSubclassOf(typeof(UIElement))) {
                    continue;
                }

                validEntryPoints.Add(entryPoint);

            }

            VertigoRuntimeLauncher behavior = (VertigoRuntimeLauncher) target;
            behavior.validEntryPointTypes = validEntryPoints.ToArray();
            behavior.validEntryPointNames = validEntryPoints.Select(s => s.GetTypeName()).ToArray();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            VertigoRuntimeLauncher launcher = (VertigoRuntimeLauncher) target;

            string typeName = serializedObject.FindProperty("typeName").stringValue;

            if (launcher.currentType == null) {
                if (typeName != null) {

                    Type resolved = Type.GetType(typeName);
                    if (resolved != null) {
                        launcher.currentType = resolved;
                        launcher.typeName = typeName;
                    }
                }
            }

            int index = Array.IndexOf(launcher.validEntryPointTypes, launcher.currentType);
            int newIndex = EditorGUILayout.Popup(index, launcher.validEntryPointNames);
            if (index != newIndex && newIndex != -1) {
                launcher.currentType = launcher.validEntryPointTypes[newIndex];
                launcher.typeName = launcher.currentType.AssemblyQualifiedName; //launcher.validEntryPointNames[newIndex];
                EditorSceneManager.MarkSceneDirty(launcher.gameObject.scene);
                launcher.RebuildApplication();
            }

            if (GUILayout.Button("Run")) {
                launcher.RebuildApplication();
            }

            if (GUILayout.Button("Precompile")) {
                // VertigoLoader.LoadPrecompiled();
            }
            
            serializedObject.FindProperty("typeName").stringValue = launcher.typeName;
            serializedObject.ApplyModifiedProperties();

        }

    }

#endif

    public class VertigoRuntimeLauncher : MonoBehaviour {

        [HideInInspector] public string typeName;
        [HideInInspector] public Type[] validEntryPointTypes;
        [HideInInspector] public string[] validEntryPointNames;
        [HideInInspector] public Type currentType;
        [HideInInspector] public VertigoApplication currentApplication;

        public void Start() {
            RebuildApplication();
        }

        public void RebuildApplication() {

            if (currentType == null) {
                return;
            }

            if (currentApplication != null) {
                currentApplication.Refresh(currentType);
            }
            else {
                currentApplication = new VertigoApplication("Default", ApplicationType.Game, CompilationType.Dynamic, currentType);
                currentApplication.Initialize(currentType);
            }

        }

        public void Update() {

            currentApplication?.RunFrame();

            // resource loading & unloading
            // compile outside play mode
            // report errors properly
            // things like custom painters or layout boxes
            // app mode (headless | editor | runtime)
            // managing built in assets
            // shaders mostly, maybe a default font. everything else comes in packages

            // tooling layer
            //    debug view per system

        }


    }

}