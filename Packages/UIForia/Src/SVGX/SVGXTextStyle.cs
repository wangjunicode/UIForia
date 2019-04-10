using TMPro;
using UIForia.Text;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;
using WhitespaceMode = UIForia.Text.WhitespaceMode;

namespace SVGX {

    public struct SVGXTextStyle {

        public float fontSize;
        public FontStyle fontStyle;
        public TextAlignment alignment;
        public TextTransform textTransform;
        public WhitespaceMode whitespaceMode;
        public Color32 outlineColor; // todo -- remove outline and make that a stroke
        public float outlineWidth;
        public float outlineSoftness;
        public float glowOuter;
        public float glowOffset;
        public TMP_FontAsset font;

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
            this.font = toClone.font;
        }
    }

}