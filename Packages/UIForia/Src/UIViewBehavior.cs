using System;
using SVGX;
using UIForia.Elements;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia {

    public class Painter : ISVGXElementPainter {

        public void Paint(UIElement element, ImmediateRenderContext ctx, SVGXMatrix matrix) {
            ctx.SetFill(Color.red);
            ctx.FillRect(400, 400, 200, 200);
        }

    }

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
            Application.RegisterCustomPainter("Painter0", new Painter());
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