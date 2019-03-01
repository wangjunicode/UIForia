using System;
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

        public TreeViewState state;
        public HierarchyView treeView;
        public UIView targetView;
        public bool playing;
        private bool needsReload;
        private RenderTexture renderTexture;
        private Camera camera;
        private Vector2 panning;
        private Mesh mesh;
        private float zoomLevel = 0;
        private float zoomSpeed = 10;
        public static UIElement SelectedElement;

        private void OnInspectorUpdate() {
            Repaint();
        }

        private static MethodInfo s_GameWindowSizeMethod;
        public static UIView UIView;

        public void OnEnable() {
            zoomLevel = 0;
            panning = Vector2.zero;
            state = new TreeViewState();
            EditorApplication.playModeStateChanged += HandlePlayState;
            autoRepaintOnSceneChange = true;
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
        }

        public void OnRefresh() {
            SelectedElement = null;
            treeView.SetRootElement(targetView.RootElement);
        }

        private void OnElementSelectionChanged(UIElement element) {
            SelectedElement = element;
        }

        private void OnDisable() {
            if (camera != null) {
                DestroyImmediate(camera.gameObject);
            }

            EditorApplication.playModeStateChanged -= HandlePlayState;
        }

        private void HandlePlayState(PlayModeStateChange obj) {
            if (obj == PlayModeStateChange.EnteredPlayMode) {
                playing = true;
                UIViewBehavior[] views = FindObjectsOfType<UIViewBehavior>();
                if (views.Length == 0) {
                    UIView = null;
                    return;
                }

                if (views[0].view?.RootElement == null) {
                    return;
                }
                
                treeView = new HierarchyView(views[0].view.RootElement, state);
                treeView.onSelectionChanged += OnElementSelectionChanged;
                targetView = views[0].view;
                UIView = targetView;
                needsReload = true;
                treeView.view = targetView;
                targetView.Application.onElementCreated += OnElementCreatedOrDestroyed;
                targetView.Application.onElementDestroyed += OnElementCreatedOrDestroyed;
                targetView.Application.onRefresh += OnRefresh;
                targetView.Application.RenderSystem.DrawDebugOverlay += HandleDrawCallback;
            }
            else if (obj == PlayModeStateChange.ExitingPlayMode) {
                playing = false;
                if (camera != null) {
                    DestroyImmediate(camera.gameObject);
                }

                if (targetView != null) {
                    targetView.Application.onElementCreated -= OnElementCreatedOrDestroyed;
                    targetView.Application.onElementDestroyed -= OnElementCreatedOrDestroyed;
                    targetView.Application.onRefresh -= OnRefresh;
                    targetView.Application.RenderSystem.DrawDebugOverlay -= HandleDrawCallback;
                }

                if (treeView != null) {
                    treeView.onSelectionChanged += OnElementSelectionChanged;
                }

                if (camera != null) {
                    DestroyImmediate(camera.gameObject);
                }

                UIView = null;
                SelectedElement = null;
            }
        }

        private void HandleDrawCallback(LightList<RenderData> renderData, LightList<RenderData> didDrawList, Vector3 origin, Camera camera) { }

        private void InitCamera() {
            if (camera != null) {
                DestroyImmediate(camera.gameObject);
            }

            GameObject cameraObject = new GameObject("UIForia Camera");
//            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10000f;
        }

        private void OnElementCreatedOrDestroyed(UIElement element) {
            needsReload = true;
        }
       
        public void OnGUI() {
            EditorGUILayout.BeginVertical();

            if (playing) {
                switch (Event.current.type) {
                    case EventType.MouseDown:
                        break;
                    case EventType.MouseUp:
                        break;
                    case EventType.MouseMove:

                        break;
                    case EventType.MouseDrag:
                        panning += new Vector2(Event.current.delta.x, -Event.current.delta.y);

                        break;
                    case EventType.KeyDown:
                        break;
                    case EventType.KeyUp:
                        break;
                    case EventType.ScrollWheel:
                        // todo -- offset camera position so it's always looking at mouse position
                        zoomLevel += (zoomSpeed * Event.current.delta.y);
                        break;
                    case EventType.Repaint:
                        break;
                    case EventType.Layout:
                        break;
                    case EventType.DragUpdated:
                        break;
                    case EventType.DragPerform:
                        break;
                    case EventType.DragExited:
                        break;
                    case EventType.Ignore:
                        break;
                    case EventType.Used:
                        break;
                    case EventType.ValidateCommand:
                        break;
                    case EventType.ExecuteCommand:
                        break;
                    case EventType.ContextClick:
                        break;
                    case EventType.MouseEnterWindow:
                        break;
                    case EventType.MouseLeaveWindow:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (treeView == null) {
                    return;
                }

                if (needsReload) {
                    needsReload = false;
                    treeView.Reload();
                    treeView.ExpandAll();
                }

                needsReload = treeView.RunGUI();
            }

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