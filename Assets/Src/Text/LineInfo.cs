using UnityEngine;

namespace Src.Text {

    public struct LineInfo {

        public int wordStart;
        public int wordCount;
        public float maxAscender;
        public float maxDescender;
        public Vector2 position;
        public float width;

        public float Height => maxAscender - maxDescender;

        public bool ContainsPoint(Vector2 point) {
            return new Rect(position.x, position.y, width, Height).Contains(point);
        }

    }


}