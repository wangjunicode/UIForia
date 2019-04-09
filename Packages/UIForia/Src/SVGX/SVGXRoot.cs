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

        [Range(-360, 360)] public float startAngle = 0;
        [Range(-360, 360)] public float endAngle = 180f;
        public float stepSize = 2f;
        public float radius = 100f;

        public void Start() {
            ctx = new ImmediateRenderContext();
            gfx = new GFX(camera);
            gfx2 = new GFX2(camera);
            ctx2 = gfx2.CreateContext();
            textInfo = new TextInfo2(new TextSpan("Hello World"));
        }

        public bool counterClockwise = false;

        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();
            
            ctx.SetStrokeWidth(strokeWidth);

            Vector2 center = new Vector2(400, 400);

            stepSize = Mathf.Max(1, stepSize);

            float _start = MathUtil.WrapAngleDeg(startAngle);
            float _end = MathUtil.WrapAngleDeg(endAngle);

     
            if (_start > _end) {
                float tmp = _end;
                _end = _start;
                _start = tmp;
            }
            
            if (Mathf.Abs(_start - _end) == 0) {
                _start = 0;
                _end = 359.9f;
            }

            float x0 = radius * Mathf.Cos(_start * Mathf.Deg2Rad);
            float y0 = radius * Mathf.Sin(_start * Mathf.Deg2Rad);
            ctx.MoveTo(center.x + x0, center.y + y0);

            for (float theta = _start + stepSize; theta < _end; theta += stepSize) {
                float x = radius * Mathf.Cos(theta * Mathf.Deg2Rad);
                float y = radius * Mathf.Sin(theta * Mathf.Deg2Rad);
                ctx.LineTo(center.x + x, center.y + y);
            }

            float x1 = radius * Mathf.Cos(_end * Mathf.Deg2Rad);
            float y1 = radius * Mathf.Sin(_end * Mathf.Deg2Rad);
            ctx.LineTo(center.x + x1, center.y + y1);

            ctx.Stroke();
            gfx.Render(ctx);
        }

    }

}