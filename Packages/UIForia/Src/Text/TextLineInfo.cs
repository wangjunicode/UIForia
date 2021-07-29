using UnityEngine;

namespace UIForia.Text {

    public struct TextLineInfoNew {

        public float x;
        public float y;
        public float width;
        public float height;
        public float maxWidth;
        public int runStart;
        public int runCount;
        
        public TextAlignment alignment;
        public WhitespaceMode whitespaceMode;
        public ushort lineCount;
        
        public TextLineInfoNew(int runStart, int runCount, float width, float x, TextAlignment alignment, WhitespaceMode whitespaceMode, float maxWidth) : this() {
            this.x = x;
            this.width = width;
            this.alignment = alignment;
            this.runStart = runStart;
            this.runCount = runCount;
            this.maxWidth = maxWidth;
            this.lineCount = 1; // todo -- remove 
            this.whitespaceMode = whitespaceMode;
        }

    }
    
}