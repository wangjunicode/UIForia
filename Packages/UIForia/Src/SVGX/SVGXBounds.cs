using UnityEngine;

namespace SVGX {

    public struct SVGXBounds {

        public Vector2 min;
        public Vector2 max;

        public bool Intersects(SVGXBounds bounds) {
            float r1x1 = min.x;
            float r1x2 = max.x;
            float r2x1 = bounds.min.x;
            float r2x2 = bounds.max.x;
            float r1y1 = min.y;
            float r1y2 = max.y;
            float r2y1 = bounds.min.y;
            float r2y2 = bounds.max.y;
            bool noOverlap = r1x1 > r2x2 ||
                             r2x1 > r1x2 ||
                             r1y1 > r2y2 ||
                             r2y1 > r1y2;
            return !noOverlap;
        }

    }

}