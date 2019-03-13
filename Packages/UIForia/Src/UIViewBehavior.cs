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
    
    public class MyPainter : ISVGXElementPainter {

        public void Paint(UIElement element, ImmediateRenderContext ctx, SVGXMatrix matrix) {
            ctx.SetStroke(Color.magenta);
            ctx.MoveTo(element.layoutResult.ActualWidth * 0.5f, 0);
            ctx.VerticalLineTo(element.layoutResult.ActualHeight);
            ctx.Stroke();
        }
    } 
    
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
            Application.RegisterCustomPainter("Painter0", new Painter());
            Application.RegisterCustomPainter("Painter1", new MyPainter());
        }

        private void Update() {
            if (type == null) return;
            application.GetView(0).Viewport = new Rect(0, 0, Screen.width, Screen.height);
            application.Update();
        }

        public void RefreshView() {
            if (type == null) return;
            view?.Refresh();
        }

    }

}