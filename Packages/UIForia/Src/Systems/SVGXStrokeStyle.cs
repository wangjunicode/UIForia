using SVGX;
using UIForia.Rendering;
using UnityEngine;
using Vertigo;

namespace Src.Systems {

    internal struct SVGXStrokeStyle {

        public PaintMode paintMode;
        public float encodedColor;
        public Texture texture;
        public SVGXMatrix uvTransform;
        public float opacity;
        public float encodedTint;
        public int gradientId;
        public float strokeWidth;
        public Vertigo.LineJoin lineJoin;
        public Vertigo.LineCap lineCap;
        public float miterLimit;

        public static SVGXStrokeStyle Default => new SVGXStrokeStyle() {
            paintMode = PaintMode.Color,
            encodedColor = VertigoUtil.ColorToFloat(Color.black),
            texture = null,
            uvTransform = SVGXMatrix.identity,
            opacity = 1f,
            encodedTint = VertigoUtil.ColorToFloat(Color.clear),
            gradientId = -1,
            lineJoin = Vertigo.LineJoin.Miter,
            lineCap = Vertigo.LineCap.Butt,
            miterLimit = 10f
        };

    }

}