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
            ctx.MoveTo(element.layoutResult.screenPosition.x + element.layoutResult.ActualWidth * 0.5f, 0);
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
            application = GameApplication.Create(applicationId, type, camera);
            Application.RegisterCustomPainter("Painter0", new Painter());
            Application.RegisterCustomPainter("Painter1", new MyPainter());
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