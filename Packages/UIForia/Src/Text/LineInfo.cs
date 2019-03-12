using UnityEngine;

namespace UIForia.Text {

    public struct LineInfo {

        public int wordStart;
        public int wordCount;
        public Vector2 position;
        public float width;
        public float height;

        public float MaxY => position.y + height;

    }


}