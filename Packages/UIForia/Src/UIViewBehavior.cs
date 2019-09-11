using System;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia {

    public class UIViewBehavior : MonoBehaviour {

        public Type type;
        public string typeName;
        public new Camera camera;
        private Path2D path;
        private Application application;
        public string applicationId = "Game App";


        public void Start() {
            type = Type.GetType(typeName);

            if (type == null) return;
            application = GameApplication.Create(applicationId, type, camera);
            application.RenderSystem.DrawDebugOverlay2 += DrawOverlay;
            path = new Path2D();
        }


        private void DrawOverlay(RenderContext ctx) {
            path.Clear();

            ctx.DrawPath(path);
        }

        private void Update() {
            if (type == null) return;
            application?.Update();
        }

    }

}