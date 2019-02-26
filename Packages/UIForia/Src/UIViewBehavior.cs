using System;
using UnityEngine;

namespace UIForia {

        public class UIViewBehavior : MonoBehaviour {

        public UIView view;
        public Type type;
        public string typeName;
        public new Camera camera;
        
        public void Start() {
            type = Type.GetType(typeName);
            
            if (type == null) return;

            view = Application.Game.AddView(new Rect(0, 0, Screen.width, Screen.height), type);
            Application.Game.SetCamera(camera);
        }

        private void Update() {
            if (type == null) return;
            Application.Game.Update();
        }

        public void RefreshView() {
            if (type == null) return;
            view?.Refresh();
        }

    }

}