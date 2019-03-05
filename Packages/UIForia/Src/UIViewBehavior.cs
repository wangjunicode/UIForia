using System;
using UnityEngine;

namespace UIForia {

    public class UIViewBehavior : MonoBehaviour {

        public UIView view;
        public Type type;
        public string typeName;
        public new Camera camera;

        private Application application;
        public string applicationId = "Game App";

        public void Start() {
            type = Type.GetType(typeName);

            if (type == null) return;
            application = new GameApplication(applicationId);
            view = application.AddView(new Rect(0, 0, Screen.width, Screen.height), type);
            application.SetCamera(camera);
        }

        private void Update() {
            if (type == null) return;
            application.Update();
        }

        public void RefreshView() {
            if (type == null) return;
            view?.Refresh();
        }

    }

}