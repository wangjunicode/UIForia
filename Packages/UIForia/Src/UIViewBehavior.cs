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

        public Vector2 pos = new Vector2(200, 200);
        public Vector2 size = new Vector2(200, 200);
        public Vector4 radii = new Vector4(100, 100, 0, 50);
        public Vector2 shadowSize;
        public Vector2 shadowOffset;
        public float shadowIntensity = 10;

        public void Start() {
            type = Type.GetType(typeName);

            if (type == null) return;
            application = GameApplication.Create(applicationId, type, camera);
            application.RenderSystem.DrawDebugOverlay2 += DrawOverlay;
        }

        private void DrawOverlay(RenderContext ctx) {
            path.Clear();
            path.SetShadowIntensity(shadowIntensity);
            path.SetShadowOffset(shadowOffset);
            path.SetShadowColor(Color.red);
            //path.RoundedRect(pos.x, pos.y, size.x, size.y, radii.x, radii.y, radii.z, radii.w);
            
            path.SetFill(Color.yellow);
            path.RoundedRect(pos.x, pos.y, size.x, size.y, radii.x, radii.y, radii.z, radii.w);
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