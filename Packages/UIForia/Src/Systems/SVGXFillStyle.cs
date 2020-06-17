using SVGX;
using UIForia.Rendering;
using UnityEngine;
using Vertigo;

namespace Src.Systems {

    internal struct SVGXFillStyle {

        public PaintMode paintMode;
        public float encodedColor;
        public Texture texture;
        public SVGXMatrix uvTransform;
        public float opacity;
        public float encodedTint;
        public int gradientId;
        public Color32 shadowColor;
        public float shadowIntensity;
        public float shadowOffsetX;
        public float shadowOffsetY;
        public float shadowSizeX;
        public float shadowSizeY;
        public Color32 shadowTint;
        public float shadowOpacity;

        public static SVGXFillStyle Default => new SVGXFillStyle() {
            paintMode = PaintMode.Color,
            encodedColor = VertigoUtil.ColorToFloat(Color.black),
            texture = null,
            uvTransform = SVGXMatrix.identity,
            opacity = 1f,
            encodedTint = VertigoUtil.ColorToFloat(Color.clear),
            gradientId = -1,
            shadowOpacity = 1
        };

    }

}