using TMPro;
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

        public void Start() {
            ctx = new ImmediateRenderContext();
            gfx = new GFX(camera);
        }

        public Texture2D texture;

        public float strokeWidth = 5;
        public Color tintColor;
        [Range(0, 1)] public float strokeOpacity;
        [Range(0, 1)] public float fillOpacity;

        public ColorStop[] gradientStops;
        public float outlineWidth = 0.25f;
        public Color textOutline;
        public float radiusTL;
        public float radiusTR;
        public float radiusBL;
        public float radiusBR;
        public float radiusTL2;
        public float radiusTR2;
        public float radiusBL2;
        public float radiusBR2;
        public float skewX = 0;
        public Color fillColor = Color.yellow;

        public Vector2 thing1;
        public Vector2 thing2;
        public bool stroke = true;
        public bool fill = true;

        public StrokePlacement strokePlacement;

        public static Vector2 Thing1;
        public static Vector2 Thing2;

        public Color shadowColor = Color.black;
        public Color shadowTint = Color.clear;
        public float shadowIntensity = 0.4f;
        [Range(0, 1)] public float shadowSoftnessX = 0.16f;
        [Range(0, 1)] public float shadowSoftnessY = 0.16f;
        public Vector2 shadowOffset;
        public Rect shadowRect = new Rect(400, 340, 200, 200);
        
        public void Update() {
            Thing1 = thing1;
            Thing2 = thing2;
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();

            ctx.SetStrokeColor(tintColor);
            ctx.SetStrokeWidth(strokeWidth);
            ctx.SetStrokePlacement(strokePlacement);
            SVGXMatrix matrix = SVGXMatrix.TRS(new Vector2(100, 100), rotation, Vector2.one); //identity);
//            matrix = matrix.SkewX(skewX);
//            ctx.SetTransform(matrix);

      
           ctx.BeginPath();
           ctx.Rect(shadowRect.x, shadowRect.y, shadowRect.width, shadowRect.height);
           ctx.SetShadowColor(shadowColor);
           ctx.SetShadowOffsetX(shadowOffset.x);
           ctx.SetShadowOffsetY(shadowOffset.y);
           ctx.SetShadowSoftnessX(shadowSoftnessX);
           ctx.SetShadowSoftnessY(shadowSoftnessY);
           ctx.SetShadowIntensity(shadowIntensity);
           ctx.SetShadowTint(shadowTint);
           
           ctx.Shadow();

           ctx.BeginPath();
           ctx.SetFill(fillColor);

           // ctx.Ellipse(100, 100, 400, 200);
           // ctx.Circle(825, 250, 100);
           ctx.Rect(400, 340, 200, 200);
           // ctx.RoundedRect(new Rect(50, 550, 300, 200), radiusTL, radiusTR, radiusBL, radiusBR);

            if (fill) {
                ctx.Fill();
            }

            if (stroke) {
//                ctx.Stroke();
//                ctx.SetStrokePlacement(StrokePlacement.Center);
//                ctx.SetStrokeColor(Color.cyan);
//                ctx.SetStrokeWidth(strokeWidth * 0.5f);
                ctx.Stroke();
            }

 
            
//            SVGXTextStyle textStyle = new SVGXTextStyle() {
//                fontSize = 72,
//                color = Color.yellow,
//                outlineColor =  textOutline,
//                outlineWidth = outlineWidth
//            };
//            
//            ctx.SetFill(Color.yellow);
//            ctx.Text(0, 0, TextUtil.CreateTextInfo(new TextUtil.TextSpan(TMP_FontAsset.defaultFontAsset, textStyle, "Hello World")));
//            ctx.Text(100, 200, TextUtil.CreateTextInfo(new TextUtil.TextSpan(TMP_FontAsset.defaultFontAsset, textStyle, "Goodbye Boston")));
////            ctx.Stroke();
//            ctx.Fill();

//            ctx.SetFill(new Color(1, 0, 0, 0.5f));
//
//            ctx.FillRect(0, 0, 100, 100);
//
//            ctx.SetFill(new Color(1, 1, 0, 0.5f));
//            ctx.FillRect(25, 25, 100, 100);

            gfx.Render(ctx);
        }

    }

}