using System.Linq;

namespace Src.Editor {

    using UnityEngine;
    using UnityEditor;

    public class CameraViewer : EditorWindow {

        RenderTexture renderTexture;

        [MenuItem("Example/Camera viewer")]
        static void Init() {
            EditorWindow editorWindow = GetWindow(typeof(CameraViewer));
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.Show();
        }

        public void Awake() {
            renderTexture = new RenderTexture((int) position.width,
                (int) position.height,
                (int) RenderTextureFormat.ARGB32);
        }

        public void Update() {
            Camera camera = FindObjectsOfType<Camera>().FirstOrDefault(c => c.name.Contains("Debug"));

            if (renderTexture == null || (renderTexture.width != position.width ||
                                          renderTexture.height != position.height)) {
                renderTexture = new RenderTexture((int) position.width,
                    (int) position.height,
                    (int) RenderTextureFormat.ARGB32);
            }

            if (camera != null) {
                Canvas canvas = FindObjectsOfType<Canvas>().FirstOrDefault();
                Camera old = canvas.worldCamera;
                canvas.worldCamera = camera;
//                camera.orthographicSize = 5;
                camera.targetTexture = renderTexture;
                camera.Render();
                camera.targetTexture = null;
                canvas.worldCamera = old;
            }
        }

        void OnGUI() {
            if (renderTexture != null) {
                GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), renderTexture);
            }
        }

    }

}