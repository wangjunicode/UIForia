using TMPro;
using UnityEngine;

namespace UIForia.Text {

    public struct CharInfo2 {

        public Vector2 topLeft;
        public Vector2 bottomRight;
        public float scale;
        public int character;
        // todo -- pull glyph & adjustment into their own data structure, not part of charinfo
        public TextGlyph glyph;
        public GlyphValueRecord glyphAdjustment;
        public Vector2 topLeftUV;
        public Vector2 bottomRightUV;
        public float topShear;
        public float bottomShear;
        public float layoutX;
        public float layoutY;
        public int lineIndex;
        public bool visible;

    }

}