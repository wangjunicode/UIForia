using System;
using System.Collections.Generic;
using System.Reflection;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UIForia.Editor {

    public class UIForiaHierarchyWindow : EditorWindow {

        public const string k_InspectedAppKey = "UIForia.Inspector.ApplicationName";

        public TreeViewState state;
        public HierarchyView treeView;
        private bool needsReload;
        private string inspectedAppId;
        private bool firstLoad;

        private void OnInspectorUpdate() {
            Repaint();
        }

        private static MethodInfo s_GameWindowSizeMethod;
        private List<Application> applications;

        public static int s_SelectedElementId;
        public static Application s_SelectedApplication;

        public void OnEnable() {
            firstLoad = true;
            state = new TreeViewState();
            autoRepaintOnSceneChange = true;
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            applications = new List<Application>();
            Application.onApplicationCreated += OnApplicationCreated;
            Application.onApplicationDestroyed += OnApplicationDestroyed;
        }

        private void OnElementSelectionChanged(UIElement element) {
            s_SelectedElementId = element.id;
        }

        private void OnDisable() {
            Application.onApplicationCreated -= OnApplicationCreated;
            Application.onApplicationDestroyed -= OnApplicationDestroyed;
        }

        private void OnApplicationCreated(Application app) {
            applications.Add(app);
        }

        private void OnApplicationDestroyed(Application app) {
            applications.Remove(app);
        }

        private void HandleDrawCallback(LightList<RenderData> renderData, LightList<RenderData> didDrawList, Vector3 origin, Camera camera) { }

        private void OnElementCreatedOrDestroyed(UIElement element) {
            needsReload = true;
        }

        public void OnRefresh() {
            s_SelectedElementId = -1;
            treeView?.Destroy();

            Application app = Application.Find(inspectedAppId);

            if (app == null) return;

            treeView = new HierarchyView(app.GetView(0).RootElement, state);
            treeView.onSelectionChanged += OnElementSelectionChanged;
            treeView.view = app.GetView(0);
        }

        private void Update() {
            if (!EditorApplication.isPlaying) {
                return;
            }

            Repaint();
        }

        private void SetApplication(string appId) {

            Application oldApp = Application.Find(inspectedAppId);

            if (oldApp != null) {
                oldApp.onElementCreated -= OnElementCreatedOrDestroyed;
                oldApp.onElementDestroyed -= OnElementCreatedOrDestroyed;
                oldApp.onRefresh -= OnRefresh;
                oldApp.RenderSystem.DrawDebugOverlay -= HandleDrawCallback;
            }

            treeView?.Destroy();

            inspectedAppId = appId;
            EditorPrefs.SetString(k_InspectedAppKey, appId);

            Application app = Application.Find(appId);

            if (app != null) {
                needsReload = true;

                treeView = new HierarchyView(app.GetView(0).RootElement, state);
                treeView.onSelectionChanged += OnElementSelectionChanged;
                treeView.view = app.GetView(0);

                app.onElementCreated += OnElementCreatedOrDestroyed;
                app.onElementDestroyed += OnElementCreatedOrDestroyed;
                app.onRefresh += OnRefresh;
                app.RenderSystem.DrawDebugOverlay += HandleDrawCallback;
            }

            s_SelectedApplication = app;
            s_SelectedElementId = -1;
        }

        public void OnGUI() {
            if (!EditorApplication.isPlaying) {
                EditorGUILayout.LabelField("Enter play mode to inspect a UIForia Application");
                return;
            }

            EditorGUILayout.BeginVertical();
            string[] names = new string[applications.Count + 1];
            names[0] = "None";

            int oldIdx = 0;

            for (int i = 1; i < names.Length; i++) {
                names[i] = applications[i - 1].id;
                if (names[i] == inspectedAppId) {
                    oldIdx = i;
                }
            }

            int idx = EditorGUILayout.Popup(new GUIContent("Application"), oldIdx, names);
            if (firstLoad || idx != oldIdx) {
                SetApplication(names[idx]);
                if (firstLoad) {
                    s_SelectedApplication = Application.Find(names[idx]);
                    s_SelectedElementId = -1;
                    firstLoad = false;
                }
            }

            if (s_SelectedApplication == null) {
                treeView?.Destroy();
                treeView = null;
            }
            
            if (treeView == null) {
                EditorGUILayout.EndVertical();
                return;
            }

            if (needsReload) {
                needsReload = false;
                treeView.Reload();
                treeView.ExpandAll();
            }

            needsReload = treeView.RunGUI();

            EditorGUILayout.EndVertical();
        }

        private static Vector2 GetMainGameViewSize() {
            if (s_GameWindowSizeMethod == null) {
                Type windowType = Type.GetType("UnityEditor.GameView,UnityEditor");
                s_GameWindowSizeMethod = windowType.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
            }

            return (Vector2) s_GameWindowSizeMethod.Invoke(null, null);
        }

    }

}