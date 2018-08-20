using Rendering;
using UnityEditor;
using UnityEngine;

namespace Src.Editor {

    public class UIViewWindow : EditorWindow {

        private UIView view;

        private void Update() {
            view.Update();
        }

        private void OnGUI() {
            view.Render();
        }

        public void Awake() {
            view = new UIViewIMGUI(typeof(TempUIType));
        }

        [MenuItem("UI/Editor Window")]
        static void InitWindow() {
            GetWindow(typeof(UIViewWindow)).Show();
        }

        private void OnEnable() {
            view.OnCreate();
            EditorApplication.update += Update;
        }

        private void OnDisable() {
            view.OnDestroy();
            view = null;
            EditorApplication.update -= Update;
        }

    }

}