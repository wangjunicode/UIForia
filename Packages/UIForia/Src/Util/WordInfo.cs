using System;

namespace UIForia.Text {

    [Flags]
    public enum WordType {

        Whitespace = 1 << 0,
        NewLine = 1 << 1,
        Normal = 1 << 2,
        SoftHyphen = 1 << 3

    }

    public struct WordInfo {

        public WordType type;
        public int charStart;
        public int charEnd;
        public float width;
        public float height;
        public float yOffset;
        public float x;
        public float y;

        public int LastCharacterIndex {
            get => charEnd - 1;
        }

    }

}