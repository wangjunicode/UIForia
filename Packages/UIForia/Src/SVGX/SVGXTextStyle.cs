using TMPro;
using UIForia.Rendering;
using UIForia.Text;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;
using WhitespaceMode = UIForia.Text.WhitespaceMode;

namespace SVGX {

    public struct SVGXTextStyle {

        public int fontSize;
        public Color32 color;
        public FontStyle fontStyle;
        public TextAlignment alignment;
        public TextTransform textTransform;
        public WhitespaceMode whitespaceMode;
        public Color32 outlineColor;
        public float outlineWidth;
        public float outlineSoftness;
        public float glowOuter;
        public float glowOffset;
        public TMP_FontAsset font;

    }

}