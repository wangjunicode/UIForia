using UIForia.Layout;
using UnityEngine;

namespace UIForia.Text {

    public struct LineInfo {

        public int wordStart;
        public int wordCount;
        public Vector2 position;
        public float width;
        public float height;

        public float MaxY => position.y + height;
        public int LastWordIndex => wordStart + wordCount - 1;

        public LineInfo(int wordStart, Vector2 position, float height) {
            this.width = 0;
            this.height = height;
            this.wordCount = 0;
            this.wordStart = wordStart;
            this.position = position;
        }
        
        public LineInfo(RangeInt wordRange, Vector2 position, float height) {
            this.width = 0;
            this.height = height;
            this.wordCount = 0;
            this.wordStart = wordRange.start;
            this.wordCount = wordRange.length;
            this.position = position;
        }
        
        public LineInfo(RangeInt wordRange, Vector2 position, Size size) {
            this.width = size.width;
            this.height = size.height;
            this.wordCount = 0;
            this.wordStart = wordRange.start;
            this.wordCount = wordRange.length;
            this.position = position;
        }
    }


}