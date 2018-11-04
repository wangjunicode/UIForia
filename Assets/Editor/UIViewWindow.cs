using Src.Rendering;
using UnityEditor;
using UnityEngine;

namespace Src.Editor {

    public class UIViewWindow : EditorWindow {

        private UIViewIMGUI view;
        private Vector2 mouse;

        private void OnGUI() {
            return;
            Rect viewport = new Rect(position) {
                x = 0,
                y = 20,
                height = position.height - 20
            };
            
            float start = Time.realtimeSinceStartup;
            view.SetViewRect(viewport);
            view.Update();
            float end = Time.realtimeSinceStartup - start;
            if (GUI.Button(new Rect(0, 0, 100, 20), "Refresh")) {
                view.Refresh();
            }
            
            switch (Event.current.type) {
                case EventType.MouseMove:
                    mouse = Event.current.mousePosition;
                    Repaint();
                    break;
            }
            
            GUI.Label(new Rect(116f, 2, 200, 20), position.size.ToString());
            GUI.Label(new Rect(216f, 2, 200, 20), mouse.ToString());
            GUI.Label(new Rect(316f, 2, 200, 20), "Frame Time: " + end.ToString("0.00"));
          
        }

        public void Awake() {
        //    view = view ?? new UIViewIMGUI(typeof(TempUIType));
        }

        [MenuItem("UI/Editor Window")]
        static void InitWindow() {
            return;
            GetWindow(typeof(UIViewWindow)).Show();
        }
//
//        private void OnEnable() {
//            wantsMouseMove = true;
//            wantsMouseEnterLeaveWindow = true;
//           // view = new UIViewIMGUI(typeof(TempUIType));
//            view.Initialize();
//           // EditorApplication.update += Update;
//        }
//
//        private void OnDisable() {
//            view.OnDestroy();
//            view = null;
//          //  EditorApplication.update -= Update;
//        }

    }

}