using System;
using System.Collections.Generic;
using System.Linq;
using UIForia.Elements;
using UIForia.Src;
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

        public void Start() { }

        public void RebuildApplication() {
            if (currentApplication != null) {
                // teardown here
            }

            if (currentType == null) {
                return;
            }

            currentApplication = new VertigoApplication("Default", ApplicationType.Game, CompilationType.Dynamic, currentType);
            
            currentApplication.Initialize();
            
        }

        public void Update() {

            if (currentApplication == null) {
                return;
            }

            if (currentApplication.IsCompiling) {
                // overlay some compilation UI
                return;
            }

            if (currentApplication.HasCompilationErrors) {
                return;
            }

            if (!currentApplication.IsInitialized) {
                return;
            }

            currentApplication.RunFrame();

            // resource loading & unloading
            // compile outside play mode
            // report errors properly
            // things like custom painters or layout boxes
            // app mode (headless | editor | runtime)
            // managing built in assets
            // shaders mostly, maybe a default font. everything else comes in packages

            // what is a compiled app?
            // module / entry point
            // custom styles
            // template database
            // style database
            // selector database
            // animation database
            // asset database

            // tooling layer
            //    debug view per system

            // virtual repeat
            //    -- fixed height or width
            //    -- big list
            //    -- elements only for visible items
            //    -- bindings run as part of layout? or we have an isEnabled keyFn like thing?
            //    dont allow nested templates? dont allow bindings? 
            //    need to know some layout properties before we can make elements
            //    1 frame lag? same frame but after layout runs? defer all events to next frame maybe
            //    then layout is fine
            //    or elements are just super cheap and it doesnt matter? renderer will cull effectively
            //    1 element gets rendered & laid out many many times and gets its data from the view
            //    elements can generate layout, input and render boxes dynamically per frame
            //    only 'weirdness' is that the same element id would get handed back for events 
            //    but maybe with a different index? 
            //    not selectable? 
            //    would need combinatorial styling
            //    reverse selector query? find all selectors applying to this box?
            //    while(has more vertical space) { keep laying out } 

            // diagnostics
            // memory stats
            // runtime stats
            // object count stats
            // some built in element types can be pooled for sure
            // div / text / etc
            // some sort of cross application enforcement -- maybe a couple bits in element id? turn elementId into long?
            // what does compiling mean?
            // we have an entry point
            // compile all its dependent files
            // modules, styles, etc
            // templates & styles should be in cache if were compiled before and no changes 
            // once an application is fully valid we're ready to go
            // start hydrating the templates

            // should have a dry run or offline style + template parser

            // probably a good time to get the window system in place
            // maybe checkout addressables

            // if application compiling
            // use some overlay gui to print "compiling"

            // if application had compiler errors
            // try compiling again every few seconds

            // if all is well and first frame
            // application.Initialize();

            // if all is well and not first frame
            // application.RunFrame();

        }

    }

}