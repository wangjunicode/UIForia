using Rendering;
using UnityEditor;
using UnityEngine;

namespace Src.Editor {

    public class UIViewWindow : EditorWindow {

        private UIViewIMGUI view;

        private void Update() {
            view.Update();
        }

        private void OnGUI() {
            if (GUI.Button(new Rect(0, 0, 100, 20), "Refresh")) {
                view.Refresh();
            }

            GUI.Label(new Rect(116f, 0, 200, 20), position.size.ToString());

            Rect viewport = new Rect(position) {
                x = 0,
                y = 20,
                height = position.height - 20
            };
            view.SetViewRect(viewport);
            view.Render();
        }

        public void Awake() {
            view = view ?? new UIViewIMGUI(typeof(TempUIType));
        }

        [MenuItem("UI/Editor Window")]
        static void InitWindow() {
            GetWindow(typeof(UIViewWindow)).Show();
        }

        private void OnEnable() {
            view = new UIViewIMGUI(typeof(TempUIType));
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