using SVGX;
using UnityEngine;

namespace UIForia.Text {

    public struct SpanInfo {

        public int charStart;
        public int charEnd;
        public int wordStart;
        public int wordEnd;
        public SVGXTextStyle textStyle;
        public string inputText;

        public SpanInfo(string text, SVGXTextStyle textStyle) {
            this.inputText = text;
            this.textStyle = textStyle;
            this.charStart = 0;
            this.charEnd = 0;
            this.wordStart = 0;
            this.wordEnd = 0;
        }

        public Texture2D fontTexture => textStyle.fontAsset.atlas;
        public int CharCount => charEnd - charStart;
        public int WordCount => wordEnd - wordStart;

    }

}