using System.Collections.Generic;
using TMPro;
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

        public void Update() {
            camera.orthographic = true;
            camera.orthographicSize = Screen.height * 0.5f;

            ctx.Clear();

            ctx.SetStrokeColor(tintColor);
            ctx.SetStrokeWidth(strokeWidth);
            ctx.SetStrokePlacement(strokePlacement);
            ctx.SetStrokeOpacity(strokeOpacity);
            ctx.SetFillOpacity(fillOpacity);

            SVGXMatrix matrix = SVGXMatrix.TRS(new Vector2(100, 100), rotation, Vector2.one); //identity);
            matrix = matrix.SkewX(skewX);
            matrix = matrix.SkewY(skewY);
            matrix = matrix.Rotate(rotation);

            ctx.SetTransform(matrix);

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

            ctx.Ellipse(100, 100, 400, 200);
            ctx.Circle(825, 250, 100);
            ctx.Rect(400, 340, 200, 200);
            ctx.RoundedRect(new Rect(50, 550, 300, 200), radiusTL, radiusTR, radiusBL, radiusBR);

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

            ctx.BeginPath();
            ctx.MoveTo(100, 250);
            matrix = matrix.Translate(500, -100);
            ctx.SetTransform(matrix);
            ctx.CubicCurveTo(new Vector2(100, 100), new Vector2(400, 100), new Vector2(400, 250));
            ctx.LineTo(200, 400);
            ctx.LineTo(400, 300);
            ctx.LineTo(100, 350);
            ctx.Stroke();

            ctx.BeginPath();

            SVGXTextStyle textStyle = new SVGXTextStyle() {
                fontSize = textSize,
                color = textColor,
                outlineColor = textOutline,
                outlineWidth = outlineWidth
            };
            
            ctx.SetTransform(SVGXMatrix.identity);

            TextInfo textInfo = TextUtil.CreateTextInfo(new TextUtil.TextSpan(TMP_FontAsset.defaultFontAsset, textStyle, "Hello Klang Gang!"));
            List<LineInfo> lineInfos = TextUtil.Layout(textInfo, float.MaxValue);
            textInfo.lineInfos = lineInfos.ToArray();
            textInfo.lineCount = lineInfos.Count;
            TextUtil.ApplyLineAndWordOffsets(textInfo);
            
            ctx.Text(20, 20, textInfo);
            ctx.Fill();

            gfx.Render(ctx);
        }

    }

}