using UnityEngine;

namespace SVGX {

    // https://www.w3.org/TR/SVG11/styling.html
    public struct SVGXStyle {

        public float strokeWidth;
        public float strokeOpacity;
        public float fillOpacity;
        public Color32 strokeColor;
        public Color32 strokeTintColor;
        
        public Color32 fillColor; 
        public Color32 fillTintColor; 
        public ColorMode fillColorMode;
        public ColorMode strokeColorMode;
        public int fillGradientId;
        public int fillTextureId;
        public StrokePlacement strokePlacement;
        public ShadowPosition shadowPosition;
        public float shadowOffsetX;
        public float shadowOffsetY;
        public Color shadowColor;
        public float shadowSoftnessX;
        public float shadowSoftnessY;
        public float shadowIntensity;
        public Color32 shadowTint;
        public int strokeGradientId;
        public int strokeTextureId;

        public bool IsFillTransparent {
            get { return fillOpacity < 1f || fillColor.a < 1f || fillTextureId != -1; } // todo add check for gradient alpha
        }
        
        public static SVGXStyle Default() {
            SVGXStyle retn = new SVGXStyle();
            retn.strokeOpacity = 1;
            retn.strokeColor = Color.black;
            retn.strokeColorMode = ColorMode.Color;
            retn.strokePlacement = StrokePlacement.Center;
            retn.strokeGradientId = -1;
            retn.strokeTextureId = -1;
            retn.strokeTintColor = Color.white;
            
            retn.fillOpacity = 1;
            retn.fillColorMode = ColorMode.Color;
            retn.fillColor = Color.white;
            retn.fillTintColor = Color.white;
            retn.fillGradientId = -1;
            retn.fillTextureId = -1;
            
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
            retn.strokeColorMode = style.strokeColorMode;
            retn.strokePlacement = style.strokePlacement;
            retn.strokeGradientId = style.strokeGradientId;
            retn.strokeTextureId = style.strokeTextureId;
            retn.strokeTintColor = style.strokeTintColor;
            
            retn.fillColor = style.fillColor;
            retn.fillColorMode = style.fillColorMode;
            retn.fillOpacity = style.fillOpacity;
            retn.fillTintColor = style.fillTintColor;
            retn.fillGradientId = style.fillGradientId;
            retn.fillTextureId = style.fillTextureId;
            
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