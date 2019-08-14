using System;
using System.Collections.Generic;
using Src.Systems;
using SVGX;
using UIForia.Extensions;
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
        public float shadowIntensity2 = 30;
        public Color shadowColor = Color.red;
        public Color shadowTint;
        public float radius = 45;
        public float rotation = 0;
        public float angle = 0;
        public float width = 20;
        public Color strokeColor = Color.red;
        public float strokeWidth = 3f;

        public Texture2D gradientOutput;
        public Texture2D image;
        public float shadowOpacity = 0.8f;
        public void Start() {
            type = Type.GetType(typeName);

            if (type == null) return;
            application = GameApplication.Create(applicationId, type, camera);
            application.RenderSystem.DrawDebugOverlay2 += DrawOverlay;
            gradientOutput = new Texture2D(128, 128);
            Vector2 start = new Vector2(0, 1);
            float length = 128;
            Vector2 end = start.Rotate(new Vector2(64, 64), -45).normalized * length;
            for (int x = 0; x < 128; x++) {
                for (int y = 0; y < 128; y++) {
                    Vector2 point = new Vector2(x, y);
                    Vector2 projected = point.Project(start, end);
                    float dist = Mathf.Clamp01(Vector2.Distance(projected, end) / length);
                    gradientOutput.SetPixel(x, y, gradient.Evaluate(dist));
                }
            }
            gradientOutput.Apply();
        }
        
        SVGXGradient gradient = new SVGXLinearGradient(GradientDirection.Horizontal, new List<ColorStop>() {
            new ColorStop(0, Color.red),
            new ColorStop(0.5f, Color.white),
            new ColorStop(1f, Color.blue),
        });

        public Vector2 p0 = new Vector2(100, 100);
        public Vector2 p1 = new Vector2(200, 200);
        public Vector2 p2 = new Vector2(300, 50);
        
        private void DrawOverlay(RenderContext ctx) {
            path.Clear();
            path.SetShadowIntensity(shadowIntensity);
            path.SetShadowOffset(shadowOffset);
            path.SetShadowSize(shadowSize);
            path.SetShadowColor(shadowColor);
            path.SetShadowTint(shadowTint);
            path.SetShadowOpacity(shadowOpacity);
//            path.Rect(100, 100, 512, 128);
//            path.SetFill(gradient);
//            path.Fill();
//            path.BeginPath();
//            path.MoveTo(100, 100);
//            path.LineTo(612, 228);
//            path.EndPath();
//            path.SetStroke(Color.black);
//            path.SetStrokeWidth(5f);
//            path.Stroke();
//            path.BeginPath();
            path.RoundedRect(pos.x - 40, pos.y - 40, size.x, size.y, radii.x, radii.y, radii.z, radii.w);
            path.Fill(FillMode.Shadow);
//            
//            path.RoundedRect(pos.x, pos.y, size.x, size.y, radii.x, radii.y, radii.z, radii.w);
//
//            path.Sector(300, 300, radius, rotation, angle, width);
                                         ////            
                                         ////            path.Sector(300, 300, radius, rotation, angle, width);
                                         //            path.Triangle(p0.x, p0.y, p1.x, p1.y, p2.x, p2.y);
                                         //            path.Fill(FillMode.Shadow);
             path.Fill();
            
//            path.SetStrokeWidth(strokeWidth);
//            path.SetStroke(strokeColor);
//            path.Stroke();
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