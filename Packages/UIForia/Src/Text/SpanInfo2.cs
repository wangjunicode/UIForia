using SVGX;

namespace UIForia.Text {

    public struct SpanInfo2 {

        public int charStart;
        public int charEnd;
        public int wordStart;
        public int wordEnd;
        public SVGXTextStyle textStyle;
        public string inputText;

        public SpanInfo2(string text, SVGXTextStyle textStyle) {
            this.inputText = text;
            this.textStyle = textStyle;
            this.charStart = 0;
            this.charEnd = 0;
            this.wordStart = 0;
            this.wordEnd = 0;
        }

        public int CharCount => charEnd - charStart;
        public int WordCount => wordEnd - wordStart;

    }

}