using System;
using Src.Systems;
using SVGX;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia {

    public class UIViewBehavior : MonoBehaviour {

        public UIView view;
        public Type type;
        public string typeName;
        public new Camera camera;

        private Application application;
        public string applicationId = "Game App";

        private Path2D path = new Path2D();

        public void Start() {
            type = Type.GetType(typeName);

            if (type == null) return;
            application = GameApplication.Create(applicationId, type, camera);
            application.RenderSystem.DrawDebugOverlay2 += DrawOverlay;
        }

        private void DrawOverlay(RenderContext ctx) {
            path.Clear();

            
            path.Rect(100, 100, 200, 100);
            path.SetStrokeWidth(3f);
            path.Fill();
            
//            path.Begin("sectionName");
////
////            path.SetUVTransform();
//
//            path.SetFillOpacity(0.5f);

//            path.SetFill(Color.red);
//            //path.Rect(300, 300, 200, 200);
////            path.Fill();
//            
//  
////            path.Begin("Lines");
////            
////            path.SetFill(Color.green);
//            path.SetFill(Color.red);
//
//            path.BeginPath(400, 100);
//            path.LineTo(450, 50);
//            path.LineTo(450, 250);
//            path.LineTo(150, 100);
//            path.ClosePath();
//            path.SetStrokeWidth(5);
//            path.SetStrokeJoin(Vertigo.LineJoin.Round);
//            path.Stroke();
//            
//            path.Fill();
//            
//            path.SetFill(Color.yellow);
//            path.Circle(100, 50, 100);
//            path.Circle(100, 250, 100);
//            path.Circle(100, 450, 100);
//            path.Ellipse(100, 650, 100, 50);
//            path.Fill();
//            path.SetFill(Color.cyan);
//            path.RegularPolygon(0, 0, 200, 200, 8);
//            path.Fill();
//            

////            path.BeginHole();
////            
////            path.CloseHole();
////            
//            path.Close();
//            
//            path.Fill();

////            path.SetStroke(3);
////            path.SetShapeRoundness();
////            path.EnableShadows();
////            path.Rect(pen, rect);
//         
            ctx.DrawPath(path);
        }

        private void Update() {
            if (type == null) return;
            application?.Update();
        }

    }

}