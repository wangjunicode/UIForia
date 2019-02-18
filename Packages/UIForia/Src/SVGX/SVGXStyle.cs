using UnityEngine;

namespace SVGX {

    // https://www.w3.org/TR/SVG11/styling.html
    public struct SVGXStyle {

        public float strokeWidth;
        public float strokeOpacity;
        public float fillOpacity;
        public Color strokeColor;
        public Color fillColor; 
        public Color fillTintColor; 
        public FillMode fillMode;
        public int gradientId;
        public int textureId;

        public static SVGXStyle Default() {
            SVGXStyle retn = new SVGXStyle();
            retn.strokeOpacity = 1;
            retn.fillMode = FillMode.Color;
            retn.strokeColor = Color.black;
            retn.fillOpacity = 1;
            retn.fillColor = Color.white;
            retn.fillTintColor = Color.white;
            retn.gradientId = -1;
            retn.textureId = -1;
            return retn;
        }

        public static SVGXStyle Clone(SVGXStyle style) {
            SVGXStyle retn = new SVGXStyle();
            retn.strokeColor = style.strokeColor;
            retn.strokeOpacity = style.strokeOpacity;
            retn.fillColor = style.fillColor;
            retn.fillMode = style.fillMode;
            retn.fillOpacity = style.fillOpacity;
            retn.fillTintColor = style.fillTintColor;
            retn.gradientId = style.gradientId;
            retn.textureId = style.textureId;
            return retn;
        }
    }

    
}