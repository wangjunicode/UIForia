using UnityEngine;

namespace Src.Text {

    public struct CharInfo {

        public char character;
        public Vector2 topLeft;
        public Vector2 bottomRight;
        public Vector2 shearValues;
        public Vector2 uv0;
        public Vector2 uv1;
        public Vector2 uv2;
        public Vector2 uv3;
        public float ascender;
        public float descender;
        public int lineIndex;
        public int wordIndex;

        public float Width => bottomRight.x - topLeft.x;
        public float Height => bottomRight.y - topLeft.y;
        public Vector2 Center => topLeft + new Vector2(Width * 0.5f, Height * 0.5f);

    }

}