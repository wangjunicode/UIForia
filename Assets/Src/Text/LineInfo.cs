using UnityEngine;

namespace Src.Text {

    public struct LineInfo {

        public int wordStart;
        public int wordCount;
        public float maxAscender;
        public float maxDescender;
        public Vector2 position;
        public Vector2 size;

        public float Height => maxAscender - maxDescender;

    }


}