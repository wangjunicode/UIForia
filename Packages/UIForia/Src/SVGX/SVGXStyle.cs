using UnityEngine;

namespace SVGX {

    // https://www.w3.org/TR/SVG11/styling.html
    public struct SVGXStyle {

        public LineJoin lineJoin;
        public LineCap lineCap;
        
        public float dashOffset;
        public float[] dashArray; // id
        public float miterLimit;
        public float strokeWidth;
        public float strokeOpacity;
        public float fillOpacity;
        public float opacity;
        public Color strokeColor; // or gradient id
        public Color fillColor;   // or gradient id
        
        public Vector2 uv;
        public int textureMode;
        public float dropShadow; 

    }

}
