using UIForia.Rendering;
using UIForia.Text;
using UnityEngine;
using FontStyle = UIForia.Text.FontStyle;
using TextAlignment = UIForia.Text.TextAlignment;

namespace SVGX {

    public struct SVGXTextStyle {

        public int fontSize;
        public Color32 color;
        public FontStyle fontStyle;
        public TextAlignment alignment;
        public TextTransform textTransform;
        public WhitespaceMode whitespaceMode;

    }

}