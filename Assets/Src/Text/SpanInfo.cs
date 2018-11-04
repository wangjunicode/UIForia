using Src.Rendering;
using Src.Util;
using TMPro;

namespace Src.Text {

    public struct SpanInfo {

        public int startChar;
        public int charCount;
        public TMP_FontAsset font;
        public FontStyle fontStyle;
        public int pointSize;
        public WhitespaceMode whitespaceMode;
        public float firstLineIndent;
        public float lineIndent;
        public float characterSpacing;
        public float wordSpacing;
        public float monoAdvance;
        public float fontSize;
        public int startWord;
        public int wordCount;
        public TextAlignment alignment;

    }

}