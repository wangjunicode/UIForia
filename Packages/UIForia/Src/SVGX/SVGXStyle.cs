using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SVGX {

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