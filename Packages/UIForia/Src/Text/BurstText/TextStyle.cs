using UnityEngine;

namespace UIForia.Text {

    public struct TextStyle {

        // public FontAssetInfo fontAsset; // make make this a handle or this struct gets huge
        public int fontAssetId;

        public FontStyle fontStyle;
        public TextAlignment alignment;
        public TextTransform textTransform;
        public WhitespaceMode whitespaceMode;

        public Color32 textColor;
        public Color32 outlineColor;
        public Color32 glowColor;
        public Color32 underlayColor;

        public UIFixedLength fontSize;
        public float outlineWidth;
        public float outlineSoftness;
        public float glowOuter;
        public float glowOffset;
        public float underlayX;
        public float underlayY;
        public float underlayDilate;
        public float underlaySoftness;
        public float faceDilate;
        public TextScript scriptStyle;
        public float lineHeight;

    }

}