using UnityEngine;

namespace SVGX {

    public struct Segment {

        public readonly Vector2 p0;
        public readonly Vector2 p1;
        public readonly Vector2 toNextNormalized;
        
        public Vector2 geometry0;
        public Vector2 geometry1;
        public Vector2 geometry2;
        public Vector2 geometry3;
        public Vector2 miter;
        public float miterLength;
        
        public bool isLeft;
        
        public Segment(Vector2 p0, Vector2 p1) {
            this.p0 = p0;
            this.p1 = p1;
            this.geometry0 = Vector2.zero;
            this.geometry1 = Vector2.zero;
            this.geometry2 = Vector2.zero;
            this.geometry3 = Vector2.zero;
            this.toNextNormalized = (p1 - p0).normalized;
            this.isLeft = false;
            this.miter = Vector2.zero;
            this.miterLength = 0f;
        }

    }

}