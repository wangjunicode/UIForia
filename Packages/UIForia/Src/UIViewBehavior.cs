using System;
using SVGX;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;
using Vertigo;

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
            application = GameApplication.Create(applicationId, type, camera);
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