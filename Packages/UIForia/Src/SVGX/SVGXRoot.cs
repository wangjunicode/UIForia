using UIForia.Text;
using Unity.Mathematics;
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



        public static ImmediateRenderContext CTX;
        public bool enableScissor = true;

        public Vector2 offset;

        [Range(1, 128)] public int fontSize;
        private TextInfo textInfo;

        [Range(-360, 360)] public float startAngle = 0;
        [Range(-360, 360)] public float endAngle = 180f;
        public float stepSize = 2f;
        public float radius = 100f;

        public void Start() {
            ctx = new ImmediateRenderContext();
            gfx = new GFX(camera);
            textInfo = new TextInfo("Hello World");
        }

        public Vector4 radii = new Vector4(100, 100, 100, 100);
        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();
            ctx.SetFill(Color.red);
            ctx.RoundedRect(new Rect(0, 0, 200, 200), radii.x, radii.y, radii.z, radii.w);
            ctx.Fill();
//            Vector2 p0 = new Vector2(500, 100);
////            Vector2 p0 = new Vector2(450, 450);
//            Vector2 p1 = new Vector2(100, 300);
////            Vector2 p1 = new Vector2(600, 600);
//            p1 = p1.Rotate(p0, rotation);
//            Vector2 p2 = new Vector2(400, 500);
////            Vector2 p2 = new Vector2(700, 700);
//            Vector2 p3 = new Vector2(600, 600);
//
//            ctx.MoveTo(p0);
//            ctx.LineTo(p1);
//            ctx.LineTo(p2);
//
////            ctx.Stroke();
//
//            ctx.BeginPath();
//            Vector2 toCurr = (p1 - p0).normalized;
//            Vector2 toNext = (p2 - p1).normalized;
//            Vector2 toCurrPerp = new Vector2(-toCurr.y, toCurr.x);
//            Vector2 toNextPerp = new Vector2(-toNext.y, toNext.x);
//
//            Vector2 miter = (toCurrPerp + toNextPerp).normalized;
////
//            ctx2.BeginPath();
//            ctx2.MoveTo(p0);
//            //ctx2.LineTo(p1);
//            ctx2.LineTo(p2);
//            ctx2.LineTo(p3);
//            ctx2.Stroke();
//
//            gfx2.Render();
            
//            ctx.Rect(200, 200, 400, 400);
//            ctx.SetFill(texture, Color.black);
//            ctx.Fill();
//            ctx.BeginPath();
//            ctx.SetFill(Color.red);
//            ctx.Rect(500, 500, 200, 200);
//            ctx.Fill();
            
//            ctx.SetStrokeWidth(strokeWidth);
//
//            Vector2 center = new Vector2(400, 400);
//
//            stepSize = Mathf.Max(1, stepSize);
//
//            float _start = MathUtil.WrapAngleDeg(startAngle);
//            float _end = MathUtil.WrapAngleDeg(endAngle);
//
//            if (_start > _end) {
//                float tmp = _end;
//                _end = _start;
//                _start = tmp;
//            }
//            
//            if (Mathf.Abs(_start - _end) == 0) {
//                _start = 0;
//                _end = 359.9f;
//            }
//
//            float x0 = radius * Mathf.Cos(_start * Mathf.Deg2Rad);
//            float y0 = radius * Mathf.Sin(_start * Mathf.Deg2Rad);
//            ctx.MoveTo(center.x + x0, center.y + y0);
//
//            for (float theta = _start + stepSize; theta < _end; theta += stepSize) {
//                float x = radius * Mathf.Cos(theta * Mathf.Deg2Rad);
//                float y = radius * Mathf.Sin(theta * Mathf.Deg2Rad);
//                ctx.LineTo(center.x + x, center.y + y);
//            }
//
//            float x1 = radius * Mathf.Cos(_end * Mathf.Deg2Rad);
//            float y1 = radius * Mathf.Sin(_end * Mathf.Deg2Rad);
//            ctx.LineTo(center.x + x1, center.y + y1);
//
//            ctx.Stroke();
            gfx.Render(ctx);
        }

    }

}