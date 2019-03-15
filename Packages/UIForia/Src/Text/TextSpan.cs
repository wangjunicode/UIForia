using SVGX;

namespace UIForia.Text {

    public struct TextSpan {

        public string text;

        public SVGXTextStyle style;
//        public SpanFlowType flowType;
//        public WhitespaceMode whitespaceMode;

        public TextSpan(string text, SVGXTextStyle style = default) {
            this.text = text;
            this.style = style;
        }

        public bool CollapseWhiteSpace => (style.whitespaceMode & WhitespaceMode.CollapseWhitespace) != 0;
        public bool PreserveNewlines => (style.whitespaceMode & WhitespaceMode.PreserveNewLines) != 0;

    }

}