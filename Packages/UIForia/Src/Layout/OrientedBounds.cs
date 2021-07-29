using UIForia.Util;
using Unity.Mathematics;

namespace UIForia.Layout {

    public struct OrientedBounds {

        public float2 p0;
        public float2 p1;
        public float2 p2;
        public float2 p3;

        public OrientedBounds(AxisAlignedBounds2D bound) {
            this.p0 = new float2(bound.xMin, bound.yMin);
            this.p1 = new float2(bound.xMax, bound.yMin);
            this.p2 = new float2(bound.xMax, bound.yMax);
            this.p3 = new float2(bound.xMin, bound.yMax);
        }

        public bool ContainsPoint(float2 point) {
            return PolygonUtil.PointInTriangle(point, p0, p1, p2) || PolygonUtil.PointInTriangle(point, p0, p2, p3);
        }
        
    }

}