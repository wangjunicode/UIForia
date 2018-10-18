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
        private RenderTexture sceneTexture;
        private Camera camera;
        
        private void OnInspectorUpdate() {
            Repaint();
        }

        public void OnEnable() {
            state = new TreeViewState();
            EditorApplication.playModeStateChanged += HandlePlayState;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
            sceneTexture = new RenderTexture(512, 512, 24);
//            GameObject cameraObject = new GameObject();
//            cameraObject.hideFlags = HideFlags.HideAndDontSave;
//            camera = cameraObject.AddComponent<Camera>();
//            camera.orthographic = true;
//            camera.nearClipPlane = 0.1f;
//            camera.farClipPlane = 10000f;
//            camera.orthographicSize = 700f;
        }

        private void OnSceneGUI(SceneView sceneview) {
//            treeView?.OnSceneGUI();
//            Rect oldRect = Camera.main.pixelRect;
//            var old = Handles.
//            Handles.SetCamera(new Rect(0, 20, 512, 512), Camera.main);
            //Handles.DrawCamera(new Rect(0, 20, 512, 512), Camera.main);
//            Camera.main.pixelRect = oldRect;
            Material mat = Resources.Load<Material>("Materials/UIForia");
            mat.color = Color.white;
            mat.SetVector("_ClipRect", new Vector4(-500, -500, 1000, 1000));

            Mesh mesh = MeshUtil.CreateStandardUIMesh(new Size(50f, 50f), Color.red);
                //Camera.main.ScreenToWorldPoint(new Vector3(0, Camera.main.pixelHeight, Camera.main.nearClipPlane)));

            Graphics.DrawMesh(mesh, new Vector3(0, 1400f), Quaternion.identity, mat, 0, SceneView.lastActiveSceneView.camera, 0, null, false, false, false);
            
        }

        private void OnDisable() {
            EditorApplication.playModeStateChanged -= HandlePlayState;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        private void HandlePlayState(PlayModeStateChange obj) {
//            if (obj == PlayModeStateChange.EnteredPlayMode) {
//                playing = true;
//                UIViewBehavior[] views = FindObjectsOfType<UIViewBehavior>();
//                treeView = new HierarchyView(views[0].view.RootElement, state);
//                targetView = views[0].view;
//                viewTransform = views[0].GetComponent<RectTransform>();
//                needsReload = true;
//                targetView.onElementCreated += OnElementCreated;
//                treeView.view = targetView;
//                treeView.viewTransform = viewTransform;
//            }
//            else if (obj == PlayModeStateChange.ExitingPlayMode) {
//                playing = false;
//                if (targetView != null) {
//                    targetView.onElementCreated -= OnElementCreated;
//                }
//
//                if (camera != null) {
//                    DestroyImmediate(camera.gameObject);
//                }
//                
//            }
        }

        private void OnElementCreated(UIElement element) {
            needsReload = true;
        }

        public void OnGUI() {
            EditorGUILayout.BeginVertical();

            if (playing) {
                SceneView.RepaintAll();
               

                // important: only render during the repaint event
                if (Event.current.type == EventType.Repaint) {
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