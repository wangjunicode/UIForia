using System;

namespace UIForia {

    [Serializable]
    public struct TextKerningPair {

        public int firstGlyph;
        public int secondGlyph;
        public float advance;

    }

}