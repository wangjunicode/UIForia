using System;
using System.Collections.Generic;
using Src.Systems;
using SVGX;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
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
        private BetterRectPacker packer;
        private SimpleRectPacker worse;
        private StructList<BetterRectPacker.PackedRect> rects;

        public static Vector2 s_ShadowSize;
        public static float s_ShadowIntensity;
        public static float s_ShadowOpacity;
        public static Color s_ShadowColor;
        
        public void Start() {
            type = Type.GetType(typeName);

            if (type == null) return;
            application = GameApplication.Create(applicationId, type, camera);
            application.RenderSystem.DrawDebugOverlay2 += DrawOverlay;
//            gradientOutput = new Texture2D(128, 128);
//            Vector2 start = new Vector2(0, 1);
//            float length = 128;
//            Vector2 end = start.Rotate(new Vector2(64, 64), -45).normalized * length;
//            for (int x = 0; x < 128; x++) {
//                for (int y = 0; y < 128; y++) {
//                    Vector2 point = new Vector2(x, y);
//                    Vector2 projected = point.Project(start, end);
//                    float dist = Mathf.Clamp01(Vector2.Distance(projected, end) / length);
//                    gradientOutput.SetPixel(x, y, gradient.Evaluate(dist));
//                }
//            }
//
//            gradientOutput.Apply();
//            packer = new BetterRectPacker(600, 600);
//            worse = new SimpleRectPacker(600, 600, 0);
//            
//            int count = 0;
//            Size[] sizes = new Size[50];
//
//            for (int i = 0; i < sizes.Length; i++) {
//                sizes[i] = new Size(Random.Range(60, 80), Random.Range(60, 80));
//            }
//
//            Stopwatch stopwatch = new Stopwatch();
//            stopwatch.Start();
//            for (int i = 0; i < sizes.Length; i++) {
//                if (packer.TryPackRect((int) sizes[i].width, (int) sizes[i].height, out BetterRectPacker.PackedRect rect)) {
//                    count++;
//                }
//            }
//
//            stopwatch.Stop();
//
//            Debug.Log("New -- Packed: " + count + " in " + stopwatch.ElapsedTicks + " ticks" + " with " + packer.checks + " checks");
//            stopwatch.Reset();
//            stopwatch.Start();
//            count = 0;
//            for (int i = 0; i < sizes.Length; i++) {
//                if (worse.TryPackRect((int) sizes[i].width, (int) sizes[i].height, out SimpleRectPacker.PackedRect rect)) {
//                    count++;
//                }
//            }
//
//            stopwatch.Stop();
//            Debug.Log("Old -- Packed: " + count + " in " + stopwatch.ElapsedTicks + " ticks" + " with " + worse.checks + " checks");

        }

        SVGXGradient gradient = new SVGXLinearGradient(GradientDirection.Horizontal, new List<ColorStop>() {
            new ColorStop(0, Color.red),
            new ColorStop(0.5f, Color.white),
            new ColorStop(1f, Color.blue),
        });

        public Vector2 p0 = new Vector2(100, 100);
        public Vector2 p1 = new Vector2(200, 200);
        public Vector2 p2 = new Vector2(300, 50);

        public void DrawPolygon(StructList<Vector2> pointList, Color color) {
            path.BeginPath();
            path.MoveTo(pointList[0]);
            for (int i = 1; i < pointList.size; i++) {
                path.LineTo(pointList[i]);
            }

            path.ClosePath();
            path.SetStroke(color);
            path.SetStrokeWidth(2f);
            path.Stroke();
        }

        public void DrawPolygonRect(PolyRect polyRect, Color color) {
            path.BeginPath();
            path.MoveTo(polyRect.p0);
            path.LineTo(polyRect.p1);
            path.LineTo(polyRect.p2);
            path.LineTo(polyRect.p3);

            path.ClosePath();
            path.SetStroke(color);
            path.SetStrokeWidth(2f);
            path.Stroke();
        }

        public Vector2 clipPosition = new Vector2(20, 20);

        private void DrawElement(UIElement element) {
            if (element.isDisabled) return;

            if (element.renderBox != null) {
                if (!element.renderBox.culled && element.renderBox.didRender) {
                    path.BeginPath();
                    path.Rect(element.layoutResult.ScreenRect);
                    path.SetStroke(Color.red);
                    path.Stroke();
                }
            }

            if (element.children == null) return;

            for (int i = 0; i < element.children.size; i++) {
                DrawElement(element.children[i]);
            }
        }

        private void DrawOverlay(RenderContext ctx) {
            path.Clear();
            s_ShadowSize = shadowSize;
            s_ShadowIntensity = shadowIntensity;
            s_ShadowColor = shadowColor;
            s_ShadowOpacity = shadowOpacity;
            VertigoRenderSystem renderSystem = application.RenderSystem as VertigoRenderSystem;
            LightList<ClipData> clippers = renderSystem.renderOwners[0].renderedClippers;

            for (int i = 0; i < clippers.size; i++) {
                if (clippers[i].isCulled) {
                    //   DrawPolygonRect(clippers[i].worldBounds, Color.red);
                }
                else {
                    //    DrawPolygon(clippers[i].intersected, Color.green);
                }
            }

//            path.SetFill(Color.red);
//            path.SetStroke(Color.black);
//            path.BeginPath();
//            path.SetStrokeWidth(3);
//
//            for (int i = 0; i < packer.sortedRectList.size; i++) {
//                path.BeginPath();
//                BetterRectPacker.PackedRect rect = packer.sortedRectList[i];
//                path.Rect(rect.xMin, rect.yMin, rect.xMax - rect.xMin, rect.yMax - rect.yMin);
//                path.Stroke();
//            }

//            path.Stroke();
//            path.Rect(100, 100, 400, 400);
//            path.SetFill(Color.black);
//            path.Fill();

//            initial -> clear to black
//            draw 'this' in white w/ blend off
//            for each other clip in hierarchy
//                draw 'that' in white w/ min blend
//
//            BlendState blendState = BlendState.Default;
//            DepthState depthState = DepthState.Default;
//            
//            path.SetBlendState(blendState);
//            
//            path.BeginPath();
//            path.Rect(0, 0, 400, 400);
//            path.SetFill(Color.black);
//            path.Fill(FillMode.Shadow);
//            
//            path.SetTransform(Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.Euler(0, rotation, 0), Vector3.one));
//
//            path.BeginPath();
//            path.Rect(25, 25, 200, 200);
//            path.SetFill(Color.white);
//            path.Fill();
//
//            blendState.sourceBlendMode = BlendMode.One; 
//            blendState.destBlendMode = BlendMode.One;
//            blendState.blendOp = BlendOp.Min;
//
//            path.SetTransform(Matrix4x4.Translate(new Vector3(0, 0, 0)));
//            
////            path.SetBlendState(blendState);
//           // path.SetTransform(Matrix4x4.Translate(new Vector3(0, 0, 13)));
//            //path.SetDepthState(new DepthState(false, CompareFunction.Equal));
//
//            path.BeginPath();
//            path.Rect(150, 150, 200, 200);
//            path.SetFill(Color.black);
//            path.Fill();
//            
//            path.BeginPath();
//            path.Rect(175, 175, 200, 200);
//            path.SetFill(Color.white);
//            path.Fill();
//
//            path.SetTransform(Matrix4x4.Translate(new Vector3(10, 0, rotation)));
//            path.SetDepthState(new DepthState(false, CompareFunction.Equal));
//
//            path.BeginPath();
//            path.Rect(175, 175, 200, 200);
//            path.SetFill(Color.red);
//            path.Fill();


//            DrawElement(rootElement);

//            Polygon subject = new Polygon();
//            subject.pointList = new StructList<Vector2>();
//            subject.pointList.Add(new Vector2(100, 100));
//            subject.pointList.Add(new Vector2(200, 100));
//            subject.pointList.Add(new Vector2(200, 300));
//            subject.pointList.Add(new Vector2(100, 300));
//            subject.Rotate(rotation);
//
//            Polygon clipPolygon = new Polygon();
//            clipPolygon.pointList = new StructList<Vector2>();
//
//            clipPolygon.pointList.Add(clipPosition + new Vector2(150, 150) * size);
//            clipPolygon.pointList.Add(clipPosition + new Vector2(250, 150) * size);
//            clipPolygon.pointList.Add(clipPosition + new Vector2(250, 250) * size);
//            clipPolygon.pointList.Add(clipPosition + new Vector2(150, 250) * size);
//
//            clipPolygon.Rotate(rotation);
//
//            Polygon clipMeNested = new Polygon();
//            clipMeNested.pointList = new StructList<Vector2>();
//
//            clipMeNested.pointList.Add(clipPolygon.pointList[0] + new Vector2(15, 15) * size);
//            clipMeNested.pointList.Add(clipPolygon.pointList[1] + new Vector2(25, 15) * size);
//            clipMeNested.pointList.Add(clipPolygon.pointList[2] + new Vector2(25, 25) * size);
//            clipMeNested.pointList.Add(clipPolygon.pointList[3] + new Vector2(15, 25) * size);
//            
//            Polygon result = clipPolygon.Clip(subject);
//            DrawPolygon(path, subject, Color.blue);
//            DrawPolygon(path, clipPolygon, Color.red);
//            DrawPolygon(path, clipMeNested, Color.white);
//            
//            if (result != null && result.pointList.size > 0) {
//                result = result.Clip(clipMeNested);
//                if (result != null && result.pointList.size > 0) {
//                    DrawPolygon(path, result, Color.green);
//                    Polygon bounds = result.GetScreenRect();
//                    DrawPolygon(path, bounds, Color.yellow);
//                }
//            }

            // path.SetShadowIntensity(shadowIntensity);
            // path.SetShadowOffset(shadowOffset);
            // path.SetShadowSize(shadowSize);
            // path.SetShadowColor(shadowColor);
            // path.SetShadowTint(shadowTint);
            // path.SetShadowOpacity(shadowOpacity);
//            path.Rect(100, 100, 512, 128);
//            path.SetFill(gradient);
//            path.Fill();
//            path.BeginPath();
//            path.MoveTo(100, 100);
//            path.LineTo(612, 100);
//            path.EndPath();
//            path.SetStroke(Color.black);
//            path.SetStrokeWidth(5f);
//            path.Stroke();

            //
            // path.BeginPath();
            // path.SetFill(Color.cyan);
            // path.RoundedRect(pos.x - 40, pos.y - 40, size.x, size.y, radii.x, radii.y, radii.z, radii.w);
            // path.Fill(FillMode.Shadow);
            // path.Fill();
//            
//            path.RoundedRect(pos.x, pos.y, size.x, size.y, radii.x, radii.y, radii.z, radii.w);
//
//            path.Sector(300, 300, radius, rotation, angle, width);
            ////            
            ////            path.Sector(300, 300, radius, rotation, angle, width);
//                                                    path.Triangle(p0.x, p0.y, p1.x, p1.y, p2.x, p2.y);
            //            path.Fill(FillMode.Shadow);
//             path.Fill();

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