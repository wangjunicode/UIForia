using UIForia.Util;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Systems {

    public struct OrientedBounds {

        public float2 p0;
        public float2 p1;
        public float2 p2;
        public float2 p3;

        public bool ContainsPoint(float2 point) {
            return PolygonUtil.PointInTriangle(point, p0, p1, p2) || PolygonUtil.PointInTriangle(point, p0, p2, p3);
        }
        
    }

}