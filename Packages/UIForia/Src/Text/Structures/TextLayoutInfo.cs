using System;

namespace UIForia.Text {
    [Serializable]
    public struct TextLayoutInfo {
        public FontAssetId fontAssetId;
        public float fontSize;
        public Text.FontStyle fontStyle;
        public Text.TextAlignment alignment;
        public float lineHeight;
        public float characterSpacing;
        public float wordSpacing;
        public WhitespaceMode whitespaceMode;
        public TextTransform textTransform;
        public float lineStartInset;
        public float lineEndInset;
        public TextOverflow overflow;
        public int maxLineCount;
        public VerticalAlignment verticalAlignment;
    }
}