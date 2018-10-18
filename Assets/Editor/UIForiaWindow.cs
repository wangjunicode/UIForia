using System.Collections.Generic;
using Src.Systems;
using Src.Util;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Src.Editor {

    public static class WindowMenuItems {

        [MenuItem("Window/UIForia")]
        private static void MissionEditor() {
            EditorWindow.GetWindow<UIForiaWindow>("UIForia Editor");
        }

    }

    public class UIForiaWindow : EditorWindow {

        public TreeViewState state;
        public HierarchyView treeView;
        public UIView targetView;
        public bool playing;
        private bool needsReload;
        private RectTransform viewTransform;
        private RenderTexture renderTexture;
        private Camera camera;
        private List<RenderData> m_RenderList;

        private void OnInspectorUpdate() {
            Repaint();
        }

        public void OnEnable() {
            state = new TreeViewState();
            EditorApplication.playModeStateChanged += HandlePlayState;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
            autoRepaintOnSceneChange = true;

        }
        public void Update()
        {
            // This is necessary to make the framerate normal for the editor window.
            Repaint();
        }
        private void OnSceneGUI(SceneView sceneview) {
            return;
            if (m_RenderList == null || camera == null) return;

            if (renderTexture == null || (renderTexture.width != position.width ||
                                          renderTexture.height != position.height)) {
                if (renderTexture != null) {
                    DestroyImmediate(renderTexture);
                }
                renderTexture = new RenderTexture((int) position.width,
                    (int) position.height,
                    (int) RenderTextureFormat.ARGB32);

            }

            Material mat = Resources.Load<Material>("Materials/UIForia");
            mat.color = Color.white;
            //mat.SetVector("_ClipRect", new Vector4(-500, -500, 1000, 1000));

            //Mesh mesh = MeshUtil.CreateStandardUIMesh(new Size(50f, 50f), Color.red);

            //Graphics.DrawMesh(mesh, new Vector3(0, 1400f), Quaternion.identity, mat, 0, camera, 0, null, false, false, false);
//            camera.orthographic = true;
//            camera.orthographicSize = 300;//position.height * 0.5f;
//            camera.aspect = 1;
            Vector3 origin = camera.transform.position;
            origin.x -= 0.5f * position.width;
            origin.y += 0.5f * position.height;
            origin.z = 10f;

//            SortGeometry();

            float z = -m_RenderList.Count;

            camera.targetTexture = renderTexture;
            camera.targetDisplay = 2;

            for (int i = 0; i < m_RenderList.Count; i++) {
                RenderData data = m_RenderList[i];
                LayoutResult layoutResult = data.element.layoutResult;
                
                Vector3 position = layoutResult.screenPosition;
                position.z = z++;
                position.y = -position.y;

                Rect clipRect = data.element.layoutResult.clipRect;

                if (clipRect.width <= 0 || clipRect.height <= 0) {
                    continue;
                }
                if (layoutResult.actualSize.width == 0 || layoutResult.actualSize.height == 0) {
                    continue;
                }

                float clipX = (clipRect.x - position.x) / layoutResult.actualSize.width;
                float clipY = ((clipRect.y - position.y) / layoutResult.actualSize.height);
                float clipW = clipX + (clipRect.width / layoutResult.actualSize.width);
                float clipH = clipY + (clipRect.height / layoutResult.actualSize.height);
                //   mat.SetVector("_ClipRect", new Vector4(clipX, clipY, clipW, clipH));
                Graphics.DrawMesh(data.drawable.GetMesh(), origin + position, Quaternion.identity, mat, 0, camera, 0, null, false, false, false);
            }
            camera.Render();
            camera.targetTexture = null;
        }

        private void OnDisable() {
            if (camera != null) {
                DestroyImmediate(camera.gameObject);
            }
            EditorApplication.playModeStateChanged -= HandlePlayState;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        private void HandlePlayState(PlayModeStateChange obj) {
            if (obj == PlayModeStateChange.EnteredPlayMode) {
                if (camera != null) {
                    DestroyImmediate(camera.gameObject);
                }
                GameObject cameraObject = new GameObject("UIForia Camera");
//            cameraObject.hideFlags = HideFlags.HideAndDontSave;
                camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 10000f;
                playing = true;
                UIViewBehavior[] views = FindObjectsOfType<UIViewBehavior>();
                treeView = new HierarchyView(views[0].view.RootElement, state);
                targetView = views[0].view;
                viewTransform = views[0].GetComponent<RectTransform>();
                needsReload = true;
                targetView.onElementCreated += OnElementCreated;
                treeView.view = targetView;
                treeView.viewTransform = viewTransform;
                m_RenderList = ((DirectRenderSystem) targetView.RenderSystem).GetRenderList();
            }
            else if (obj == PlayModeStateChange.ExitingPlayMode) {
                playing = false;
                if (camera != null) {
                    DestroyImmediate(camera.gameObject);
                }
                if (targetView != null) {
                    targetView.onElementCreated -= OnElementCreated;
                }

                if (camera != null) {
                    DestroyImmediate(camera.gameObject);
                }

            }
        }

        private void OnElementCreated(UIElement element) {
            needsReload = true;
        }

        public void OnGUI() {
            EditorGUILayout.BeginVertical();

            if (playing) {
                //SceneView.RepaintAll();

                // important: only render during the repaint event
               if (Event.current.type == EventType.Repaint) {
                    if (renderTexture != null) {
                        GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), renderTexture);
                    }
               }

                if (needsReload) {
                    needsReload = false;
                    treeView.Reload();
                    treeView.SetExpandedRecursive(0, true);
                }

//                treeView.OnGUI(GUILayoutUtility.GetRect(0, 10000, 0, 10000));
            }

            EditorGUILayout.EndVertical();

        }

    }

}