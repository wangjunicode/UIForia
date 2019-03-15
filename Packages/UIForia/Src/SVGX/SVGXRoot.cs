using System.Collections.Generic;
using Packages.UIForia.Src.VectorGraphics;
using TMPro;
using UIForia.Extensions;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public class SVGXRoot : MonoBehaviour {

        public new Camera camera;
        private Mesh mesh;
        private Mesh lineMesh;

        private GFX gfx;

        private ImmediateRenderContext ctx;

        [Range(0, 360f)] public float rotation;
        public float dot;


        public Texture2D texture;

        public float strokeWidth = 5;
        public Color tintColor;
        [Range(0, 1)] public float strokeOpacity;
        [Range(0, 1)] public float fillOpacity;

        public ColorStop[] gradientStops;
        public float outlineWidth = 0.25f;
        public Color textOutline;
        public int textSize;
        public Color textColor;
        public float radiusTL;
        public float radiusTR;
        public float radiusBL;
        public float radiusBR;
        public float skewX = 0;
        public float skewY = 0;
        public Color fillColor = Color.yellow;

        public Vector2 thing1;
        public Vector2 thing2;
        public bool stroke = true;
        public bool fill = true;

        public StrokePlacement strokePlacement;

        public Color shadowColor = Color.black;
        public Color shadowTint = Color.clear;
        public float shadowIntensity = 0.4f;
        [Range(0, 1)] public float shadowSoftnessX = 0.16f;
        [Range(0, 1)] public float shadowSoftnessY = 0.16f;
        public Vector2 shadowOffset;
        public Rect shadowRect = new Rect(400, 340, 200, 200);

        private GFX2 gfx2;
        private VectorContext ctx2;

        public static ImmediateRenderContext CTX;
        public bool enableScissor = true;

        public Vector2 offset;

        [Range(1, 128)] public int fontSize;
        private TextInfo2 textInfo;

        public void Start() {
            ctx = new ImmediateRenderContext();
            gfx = new GFX(camera);
            gfx2 = new GFX2(camera);
            ctx2 = gfx2.CreateContext();
            textInfo = new TextInfo2(new TextSpan("Hello World"));
        }

        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();

            if (enableScissor) {
                ctx.EnableScissorRect(new Rect(100, 100, 300, 300));
            }

            ctx.SetFill(Color.green);
            ctx.RoundedRect(new Rect(100, 100, 300, 100), 200, 200, 200, 200);
            ctx.Fill();
            ctx.BeginPath();
            ctx.SetFill(Color.green);
            ctx.RoundedRect(new Rect(100, 200, 300, 100), 20, 20, 20, 20);
            ctx.Fill();
            
//            ctx.Rect(new Rect(200, 200, 200, 200));
//            ctx.Rect(new Rect(200, 400, 200, 200));
            ctx.Fill();
            
            textInfo.SetSpanStyle(0, new SVGXTextStyle() {
                fontSize = fontSize
            });

//            textInfo.Layout();
//
//            ctx.Text(100 + offset.x, 100 + offset.y, textInfo);
//            ctx.SetFill(Color.black);
//            ctx.Fill();
//
//            ctx.SetFill(new Color(0, 255, 0, 255));
//            ctx.FillRect(100, 100, 200, 200);
//            ctx.SetFill(new Color32(255, 0, 0, 100));
//            ctx.FillRect(150, 150, 200, 200);
            gfx.Render(ctx);
        }

    }

}