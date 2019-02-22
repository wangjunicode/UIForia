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
        
        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();

//            SVGXGradient gradient = new SVGXLinearGradient(GradientDirection.Vertical, gradientStops);

//            ctx.Circle(0, 0, 100);
//            ctx.PushClip();

//            ctx.SetFill(texture);
//            ctx.FillRect(new Rect(-50, -50, 300, 300));

//            ctx.BeginPath();
//            ctx.SetStrokeColor(tintColor);
//            ctx.SetStrokeWidth(strokeWidth);
//            ctx.SetStrokeOpacity(strokeOpacity);
////            ctx.Circle(0, 0, 300);
//            ctx.LineTo(400, 400);
//            ctx.LineTo(400, 0);
//            ctx.Stroke();
//
//            ctx.BeginPath();
//            ctx.Rect(0, 0, 300, 300);
//            ctx.SetFill(Color.yellow);
//            ctx.Fill();

     
            
            ctx.MoveTo(200f * 0.5f, 0);
            ctx.LineTo(200f * 0.5f, 10f);
            ctx.SetStrokeWidth(200f);
            ctx.SetStrokeColor(Color.red);
            ctx.SetStrokePlacement(StrokePlacement.Outside);
            ctx.SetStrokeOpacity(1f);
            ctx.Stroke();
            
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