using UnityEngine;

namespace SVGX {

    // https://www.w3.org/TR/SVG11/styling.html
    public struct SVGXStyle {

        public float strokeWidth;
        public float strokeOpacity;
        public float fillOpacity;
        public Color32 strokeColor;
        public Color32 fillColor; 
        public Color32 fillTintColor; 
        public FillMode fillMode;
        public int gradientId;
        public int textureId;
        public StrokePlacement strokePlacement;
        public ShadowPosition shadowPosition;
        public float shadowOffsetX;
        public float shadowOffsetY;
        public Color shadowColor;
        public float shadowSoftnessX;
        public float shadowSoftnessY;
        public float shadowIntensity;
        public Color32 shadowTint;

        public bool IsFillTransparent {
            get { return fillOpacity < 1f || fillColor.a < 1f || textureId != -1; } // todo add check for gradient alpha
        }
        
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
            retn.strokePlacement = StrokePlacement.Center;
            retn.shadowColor = Color.black;
            retn.shadowOffsetX = 0;
            retn.shadowOffsetY = 0;
            retn.shadowSoftnessX = 0.16f;
            retn.shadowSoftnessY = 0.16f;
            retn.shadowIntensity = 0.3f;
            retn.shadowTint = Color.clear;
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
            retn.strokePlacement = style.strokePlacement;
            retn.shadowColor = style.shadowColor;
            retn.shadowOffsetX = style.shadowOffsetX;
            retn.shadowOffsetY = style.shadowOffsetY;
            retn.shadowSoftnessX = style.shadowSoftnessX;
            retn.shadowSoftnessY = style.shadowSoftnessY;
            retn.shadowIntensity = style.shadowIntensity;
            retn.shadowTint = style.shadowTint;
            return retn;
        }
    }

    
}