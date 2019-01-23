using System.Collections.Generic;
using UnityEngine;

namespace SVGX {

    public static class SVGXArc {

         public static List<Vector2> Arc(Vector2 p1, float rx, float ry, float angle, bool largeArcFlag, bool sweepFlag, Vector2 p2) {
            
            List<Vector2> output = new List<Vector2>();

            float _radian = (angle * Mathf.PI / 180.0f);
            float _CosRadian = Mathf.Cos(_radian);
            float _SinRadian = Mathf.Sin(_radian);
            float temp1 = (p1.x - p2.x) / 2.0f;
            float temp2 = (p1.y - p2.y) / 2.0f;
            float tx = (_CosRadian * temp1) + (_SinRadian * temp2);
            float ty = (-_SinRadian * temp1) + (_CosRadian * temp2);
            
            double trx2 = rx * rx;
            double try2 = ry * ry;
            double tx2 = tx * tx;
            double ty2 = ty * ty;
                    
            double radiiCheck = tx2 / trx2 + ty2 / try2;
            if(radiiCheck > 1) {
                rx = Mathf.Sqrt((float)radiiCheck) * rx;
                ry = Mathf.Sqrt((float)radiiCheck) * ry;
                trx2 = rx * rx;
                try2 = ry * ry;
            }

            double tm1 = (trx2 * try2 - trx2 * ty2 - try2 * tx2) / (trx2 * ty2 + try2 * tx2);
            tm1 = (tm1 < 0) ? 0 : tm1;

            float tm2 = (largeArcFlag == sweepFlag) ? -Mathf.Sqrt((float)tm1) : Mathf.Sqrt((float)tm1);

            float tcx = tm2 * ((rx * ty) / ry);
            float tcy = tm2 * (-(ry * tx) / rx);

            float cx = _CosRadian * tcx - _SinRadian * tcy + ((p1.x + p2.x) / 2.0f);
            float cy = _SinRadian * tcx + _CosRadian * tcy + ((p1.y + p2.y) / 2.0f);
            
            float ux = (tx - tcx) / rx;
            float uy = (ty - tcy) / ry;
            float vx = (-tx - tcx) / rx;
            float vy = (-ty - tcy) / ry;

            float n = Mathf.Sqrt((ux * ux) + (uy * uy));
            float p = ux;
            float _angle = (uy < 0) ? -Mathf.Acos(p / n) : Mathf.Acos(p / n);
            _angle = _angle * 180.0f / Mathf.PI;
            _angle %= 360f;
            
            n = Mathf.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
            p = ux * vx + uy * vy;
            var t = p / n;
            if((Mathf.Abs(t) >= 0.99999f) && (Mathf.Abs(t) < 1.000009f)) {
                if(t > 0)
                    t = 1f;
                else
                    t = -1f;
            }
            var _delta = (ux * vy - uy * vx < 0) ? -Mathf.Acos(t) : Mathf.Acos(t);
            
            _delta = _delta * 180.0f / Mathf.PI;
            
            if(!sweepFlag && _delta > 0) {
                _delta -= 360f;
            } else if(sweepFlag && _delta < 0)
                _delta += 360f;
            
            _delta %= 360f;

            int number = Mathf.RoundToInt( Mathf.Clamp((100f / 1000f) * Mathf.Abs(_delta) / 360f, 2, 100));
            float deltaT = _delta / number;
            
            Vector2 _point = new Vector2(0, 0);
            for(int i = 0; i <= number; i++) {
                var t_angle = (deltaT * i + _angle) * Mathf.PI / 180.0f;
                _point.x = _CosRadian * rx * Mathf.Cos(t_angle) - _SinRadian * ry * Mathf.Sin(t_angle) + cx;
                _point.y = _SinRadian * rx * Mathf.Cos(t_angle) + _CosRadian * ry * Mathf.Sin(t_angle) + cy;
                output.Add(_point);
            }
            
            return output;
        }

    }
    
    public static class SVGXBezier {

        private const int MaxAdaptiveBezierIteration = 200;

        public static List<Vector2> QuadraticCurve(Vector2 p1, Vector2 p2, Vector2 p3) {
            Vector2 ctrl2 = p1 + (2f / 3f) * (p2 - p1);
            Vector2 ctrl3 = p3 + (2f / 3f) * (p2 - p3);
            
            return new List<Vector2>(AdaptiveCubicCurve(1, p1, ctrl2, ctrl3, p3));
        }

        
        // todo -- dont return a new list
        public static List<Vector2> Tessellate(Vector2 start, Vector2 ctrl0, Vector2 ctrl1, Vector2 end) {
            return AdaptiveCubicCurve(1f, start, ctrl0, ctrl1, end);
        }

        public static List<Vector2> AdaptiveCubicCurve(float distanceTolerance, Vector2 start, Vector2 handle0, Vector2 handle1, Vector2 end) {
            if (start == handle0 && handle0 == handle1 && handle1 == end) {
                return new List<Vector2>();
            }

            if (distanceTolerance < 0.01f) {
                distanceTolerance = 0.01f;
            }

            List<Vector2> points = new List<Vector2>(40);
            points.Add(start);

            RecursiveBezier(
                points,
                0,
                distanceTolerance * distanceTolerance,
                start.x, start.y,
                handle0.x, handle0.y,
                handle1.x, handle1.y,
                end.x, end.y
            );

            points.Add(end);
            return points;
        }


        private static void RecursiveBezier(List<Vector2> points, int currentIteration, float distanceTolerance, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) {
            while (true) {
                if (currentIteration++ >= MaxAdaptiveBezierIteration) return;

                // Calculate all the mid-points of the line segments
                //----------------------
                float x12 = (x1 + x2) * 0.5f;
                float y12 = (y1 + y2) * 0.5f;
                float x23 = (x2 + x3) * 0.5f;
                float y23 = (y2 + y3) * 0.5f;
                float x34 = (x3 + x4) * 0.5f;
                float y34 = (y3 + y4) * 0.5f;
                float x123 = (x12 + x23) * 0.5f;
                float y123 = (y12 + y23) * 0.5f;
                float x234 = (x23 + x34) * 0.5f;
                float y234 = (y23 + y34) * 0.5f;
                float x1234 = (x123 + x234) * 0.5f;
                float y1234 = (y123 + y234) * 0.5f;

                // Try to approximate the full cubic curve by a single straight line
                //------------------
                float dx = x4 - x1;
                float dy = y4 - y1;

                float d2 = Mathf.Abs(((x2 - x4) * dy - (y2 - y4) * dx));
                float d3 = Mathf.Abs(((x3 - x4) * dy - (y3 - y4) * dx));

                if ((d2 + d3) * (d2 + d3) < distanceTolerance * (dx * dx + dy * dy)) {
                    points.Add(new Vector2(x1234, y1234));
                    return;
                }

                // Continue subdivision
                //----------------------
                RecursiveBezier(points, currentIteration, distanceTolerance, x1, y1, x12, y12, x123, y123, x1234, y1234);
                x1 = x1234;
                y1 = y1234;
                x2 = x234;
                y2 = y234;
                x3 = x34;
                y3 = y34;
            }
        }

    }

}