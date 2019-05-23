using UIForia;
using UIForia.Text;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace SVGX {

    public struct SVGXTextStyle {

        public float fontSize;
        public FontStyle fontStyle;
        public TextAlignment alignment;
        public TextTransform textTransform;
        public WhitespaceMode whitespaceMode;
        public Color32 outlineColor;
        public float outlineWidth;
        public float outlineSoftness;
        public float glowOuter;
        public float glowOffset;
        public FontAsset fontAsset;
        public float underlayX;
        public float underlayY;
        public float underlayDilate;
        public float underlaySoftness;
        public float faceDilate;

        public SVGXTextStyle(SVGXTextStyle toClone) {
            this.fontSize = toClone.fontSize;
            this.fontStyle = toClone.fontStyle;
            this.alignment = toClone.alignment;
            this.textTransform = toClone.textTransform;
            this.whitespaceMode = toClone.whitespaceMode;
            this.outlineColor = toClone.outlineColor;
            this.outlineWidth = toClone.outlineWidth;
            this.outlineSoftness = toClone.outlineSoftness;
            this.glowOuter = toClone.glowOuter;
            this.glowOffset = toClone.glowOffset;
            this.underlayX = toClone.underlayX;
            this.underlayY = toClone.underlayY;
            this.underlayDilate = toClone.underlayDilate;
            this.underlaySoftness = toClone.underlaySoftness;
            this.fontAsset = toClone.fontAsset;
            this.faceDilate = toClone.faceDilate;
        }
    }

}