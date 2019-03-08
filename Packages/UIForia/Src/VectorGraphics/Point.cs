using UnityEngine;

namespace Packages.UIForia.Src.VectorGraphics {

    public struct Point {

        public float x;
        public float y;
        public float deltaX;
        public float deltaY;
        public float length;
        public float dmx;
        public float dmy;
        public PointFlags flags;

        public Point(Vector2 position, PointFlags flags) {
            this.x = position.x;
            this.y = position.y;
            this.deltaX = 0;
            this.deltaY = 0;
            this.length = 0;
            this.dmx = 0;
            this.dmy = 0;
            this.flags = flags;
        }
        
        public Vector2 position => new Vector2(x, y);

    }

}