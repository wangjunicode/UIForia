using UnityEngine;

namespace UIForia.Text {

    public struct TextLineInfo {

        // screen coords
        public float x;
        public float y;
        public float width;
        public float height;
        public int wordStart;
        public int wordCount; 
        

        
        public Rect LineRect => new Rect(x, y, width, height);

        // indices 
        // word index into the first span

        // ... into the last span

        // total word count across all spans in this line
        // maybe kill these?
        
        // maybe kill this?
       //  public int wordEnd;
        public int globalCharacterStartIndex;
        public int globalCharacterEndIndex;

        public int LastWordIndex => 0;

        public TextLineInfo(int wordStart, int wordCount, float width) : this() {
            this.wordStart = wordStart;
            this.wordCount = wordCount;
            this.width = width;
        }

        public TextLineInfo(int wordStart, float width = 0) {
            this.wordStart = wordStart;
            this.x = 0;
            this.y = 0;
            this.width = width;
            this.height = 0;
            this.wordCount = 0;
            this.globalCharacterStartIndex = 0;
            this.globalCharacterEndIndex = 0;
        }

    }

}
