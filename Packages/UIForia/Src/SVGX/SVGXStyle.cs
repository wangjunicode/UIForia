using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SVGX {

    [Flags]
    public enum FillMode {

        Color = 0,
        Texture = 1 << 0,
        Gradient = 1 << 1,
        Pattern = 1 << 4,
        
        TextureGradient = Texture | Gradient,
        TextureColor = Texture | Color

    }
    
    // https://www.w3.org/TR/SVG11/styling.html
    public struct SVGXStyle {

        public LineJoin lineJoin;
        public LineCap lineCap;

        public float dashOffset;
        public float[] dashArray;
        public float miterLimit;
        public float strokeWidth;
        public float strokeOpacity;
        public float fillOpacity;
        public Color strokeColor;
        public Color fillColor; 
        public Color fillTintColor; 
        public FillMode fillMode;
        public Vector2 uv;
        public float dropShadow;
        public int gradientId;
        public int textureId;
        public SVGXStrokeStyle strokeStyle;

    }

    public struct SVGXStrokeStyle {

        public Color color;
        public float alpha;
        public float width;
        public float[] dashArray;
        public LineCap lineCap;
        public LineJoin lineJoin;
        public float miterLimit;

    }

    public struct SVGXTextStyle {

        public TMP_FontAsset font;
        public int fontSize;
        public TextAlignment alignment;
        public Color color;
        public FontStyle fontStyle;
        public float outline;
        public Color outlineColor;

    }
    
}