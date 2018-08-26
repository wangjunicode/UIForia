using Debugger;
using Rendering;
using UnityEditor;
using UnityEngine;

namespace Src.Editor {

    public class DebuggerWindow : EditorWindow {

        [MenuItem("UI/Debugger Window")]
        
        private static void InitWindow() {
            GetWindow(typeof(DebuggerWindow)).Show();
        }
        
        private UIViewIMGUI view;

        private void Update() {
            view.Update();
        }

        private void OnGUI() {
   
            Rect viewport = new Rect(position) {
                x = 0,
                y = 20,
                height = position.height - 20
            };
            
            view.SetViewRect(viewport);
            view.Render();

            if (GUI.Button(new Rect(0, 0, 100, 20), "Refresh")) {
                view.Refresh();
            }
            
            switch (Event.current.type) {
                case EventType.MouseMove:
                    Repaint();
                    break;
            }
            Repaint();
   
        }

        public void Awake() {
            view = view ?? new UIViewIMGUI(typeof(TempUIType));
        }

        private void OnEnable() {
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            view = new UIViewIMGUI(typeof(Inspector));
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