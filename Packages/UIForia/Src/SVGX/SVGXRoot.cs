using System.Collections.Generic;
using UIForia.Extensions;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public class SVGXRoot : MonoBehaviour {

        public new Camera camera;
        private Mesh mesh;
        private Mesh lineMesh;

        private GFX gfx;

        [Header("Fill Materials")] public Material simpleFillOpaque;
        public Material stencilFillCutoutOpaque;
        public Material stencilFillPaintOpaque;
        public Material stencilFillClearOpaque;

        [Header("Stroke Materials")] public Material simpleStrokeOpaque;

        private ImmediateRenderContext ctx;

        [Range(0, 360f)] public float rotation;
        public float dot;

        public Texture2D texture;
        
        public void Start() {
            ctx = new ImmediateRenderContext();
            gfx = new GFX(camera) {
                simpleFillOpaqueMaterial = simpleFillOpaque,
                stencilFillOpaqueCutoutMaterial = stencilFillCutoutOpaque,
                stencilFillOpaquePaintMaterial = stencilFillPaintOpaque,
                stencilFillOpaqueClearMaterial = stencilFillClearOpaque,
                simpleStrokeOpaqueMaterial = simpleStrokeOpaque
            };
        }

        private List<float> times = new List<float>(new [] {
            0.0f,
            0.2f,
            0.4f,
            0.6f,
            0.8f,
            1f,
        });
        
        private ColorStop[] colorStops = new ColorStop[6];
        
        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();

            for (int i = 0; i < times.Count; i++) {
                times[i] += Time.deltaTime;
                if (times[i] > 1) {
                    times[i] = 1 - times[i];
                }
            }
                        
            colorStops[0] = new ColorStop(times[0], Color.red);
            colorStops[1] = new ColorStop(times[1], Color.red);
            colorStops[2] = new ColorStop(times[2], Color.white);
            colorStops[3] = new ColorStop(times[3], Color.red);
            colorStops[4] = new ColorStop(times[4], Color.blue);
            colorStops[5] = new ColorStop(times[5], Color.red);
             
            SVGXGradient gradient = new SVGXLinearGradient(GradientDirection.Horizontal, colorStops);

            ctx.SetFill(texture, gradient);
            ctx.FillEllipse(0, 0, 1000, 300);
            
//            gradient = new SVGXLinearGradient(GradientDirection.Horizontal, new[] {
//                new ColorStop(0f, new Color32(0, 0, 0, 255)),
//                new ColorStop(0.3f, new Color32(0, 0, 0, 255)),
//                new ColorStop(0.3f, new Color32(0, 0, 0, 175)),
//                new ColorStop(0.6f, new Color32(255, 0, 0, 75)),
//                new ColorStop(0.7f, new Color32(0, 0, 0, 75)),
//                new ColorStop(1f, new Color32(0, 0, 0, 0)),
//            });
            
//            ctx.SetFill(gradient);
//            ctx.FillRect(-500, -200, 1000, 300);
//            ctx.Fill();
//            
//            ctx.BeginPath();
//            ctx.SetFillColor(Color.white);
//            ctx.Rect(100, 300, 100, 100);
//            ctx.Ellipse(100, 100, 50, 100);
//            ctx.Fill();
//  
//            ctx.BeginPath();
//            ctx.SetFillColor(Color.blue);
//            ctx.Rect(300, 100, 100, 100);
//            ctx.Fill();
//            ctx.Stroke();

//            ctx.LineTo(100, 100);
//            Vector2 v = new Vector2(200, 0);
//            v = v.Rotate(new Vector2(100, 100), rotation);
//            ctx.LineTo(v.x, v.y);
//            ctx.Stroke();
//            Vector2 toCurrent = new Vector2(100, 100) - new Vector2(0, 0);
//            Vector2 toNext = v - new Vector2(100, 100);
//
//            toCurrent = toCurrent.normalized;
//            toNext = toNext.normalized;
//
//            dot = Vector2.Dot(toCurrent, new Vector2(-toNext.y, toNext.x));

            gfx.Render(ctx);
        }

    }

}