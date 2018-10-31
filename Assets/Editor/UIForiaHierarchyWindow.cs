using System;
using System.Collections.Generic;
using System.Reflection;
using Src.Systems;
using Src.Util;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Src.Editor {

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

    
        public void OnEnable() {
            zoomLevel = 0;
            panning = Vector2.zero;
            state = new TreeViewState();
            EditorApplication.playModeStateChanged += HandlePlayState;
            autoRepaintOnSceneChange = true;
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
        }

        public void Update() {
        //    Repaint();
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
                    return;
                }
                treeView = new HierarchyView(views[0].view.RootElement, state);
                treeView.onSelectionChanged += OnElementSelectionChanged;
                targetView = views[0].view;
                needsReload = true;
                targetView.onElementCreated += OnElementCreated;
                treeView.view = targetView;
            }
            else if (obj == PlayModeStateChange.ExitingPlayMode) {
                playing = false;
                if (camera != null) {
                    DestroyImmediate(camera.gameObject);
                }

                if (targetView != null) {
                    targetView.onElementCreated -= OnElementCreated;
                }

                if (treeView != null) {
                    treeView.onSelectionChanged += OnElementSelectionChanged;
                }

                if (camera != null) {
                    DestroyImmediate(camera.gameObject);
                }

                SelectedElement = null;
            }
        }

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

        private void OnElementCreated(UIElement element) {
            needsReload = true;
        }

        public void OnGUI() {
            EditorGUILayout.BeginVertical();
            //  titleContent = new GUIContent(position.width + ", " + position.height);
            // height = orthosize * 2
            // width = height*Camera.main.aspect
            // aspect = 
            if (playing) {
                //SceneView.RepaintAll();

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

                // important: only render during the repaint event
                //   if (Event.current.type == EventType.Repaint) {
//                    if (renderTexture != null) {
//                        GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), renderTexture);
//                    }
                //  }

                if (treeView == null) {
                    return;
                }
                
                if (needsReload) {
                    needsReload = false;
                    treeView.Reload();
                    treeView.SetExpandedRecursive(0, true);
                }

                treeView.OnGUI(GUILayoutUtility.GetRect(0, 10000, 0, 10000));
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
//
//        private void OnSceneGUI(SceneView sceneView) {
//            if (m_RenderList == null || camera == null) return;
//
//            if (renderTexture == null || (renderTexture.width != position.width ||
//                                          renderTexture.height != position.height)) {
//                if (renderTexture != null) {
//                    DestroyImmediate(renderTexture);
//                }
//
//                renderTexture = new RenderTexture(
//                    (int) position.width,
//                    (int) position.height,
//                    24
//                );
//            }
//
//            Material mat = Resources.Load<Material>("Materials/UIForia");
//            mat.color = Color.white;
//            Vector3 origin = camera.transform.position;
//            origin.x -= 0.5f * position.width;
//            origin.y += 0.5f * position.height;
//            origin.z = 10f;
//
////            SortGeometry();
//
//            // need to know:
//            // base scale
//            // zoom level = 1
//            // width / height
//
//            float z = -m_RenderList.Count;
////            camera.orthographicSize = Camera.main.orthographicSize;
//            camera.targetTexture = renderTexture;
//            camera.targetDisplay = 2;
//            camera.transform.position = new Vector3(0, 0, -10);
//            for (int i = 0; i < m_RenderList.Count; i++) {
//                RenderData data = m_RenderList[i];
//                LayoutResult layoutResult = data.element.layoutResult;
//
//                Vector3 position = layoutResult.screenPosition;
//                position.z = z++;
//                position.y = -position.y;
//
//                Rect clipRect = data.element.layoutResult.clipRect;
//
//                if (clipRect.width <= 0 || clipRect.height <= 0) {
//                    continue;
//                }
//
//                if (layoutResult.actualSize.width == 0 || layoutResult.actualSize.height == 0) {
//                    continue;
//                }
//
//                float clipX = (clipRect.x - position.x) / layoutResult.actualSize.width;
//                float clipY = ((clipRect.y - position.y) / layoutResult.actualSize.height);
//                float clipW = clipX + (clipRect.width / layoutResult.actualSize.width);
//                float clipH = clipY + (clipRect.height / layoutResult.actualSize.height);
//                //   mat.SetVector("_ClipRect", new Vector4(clipX, clipY, clipW, clipH));
//            }
//
//            float size = (zoomLevel) + 250f;
//            if (size < 0.5) size = 0.5f;
//            camera.orthographicSize = size;
//
//            mesh = mesh ? mesh : MeshUtil.CreateStandardUIMesh(new Size(100, 100), Color.white);
//            
//            Graphics.DrawMesh(mesh, new Vector3(-200, 200, 0) + new Vector3(panning.x, panning.y, 0), Quaternion.identity, mat, 0, camera, 0, null, false, false, false);
//            Graphics.DrawMesh(mesh, new Vector3(-200, 200, 1) + new Vector3(panning.x, panning.y, 0), Quaternion.identity, mat, 0, camera, 0, null, false, false, false);
//
//            /*
//             * draw on top of scene (or maybe into)
//             * highlight / select on mouse over and click
//             * move?
//             * move anchors
//             * see properties
//             * show clipped sections
//             * click hierarchy -> highlight view
//             */
//            
//            camera.Render();
//            camera.targetTexture = null;
//        }