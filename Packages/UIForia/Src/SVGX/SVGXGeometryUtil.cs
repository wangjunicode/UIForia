using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    public static class SVGXGeometryUtil {

        // maybe we can cache the sin/cos result if we use a standard tessellation step size
        
        public static Vector2 PointOnCircle(Vector2 origin, float radianStep, float radius) {
            return new Vector2(
                origin.x + radius * Mathf.Cos(radianStep),
                origin.y + radius * Mathf.Sin(radianStep)
            );
        }
        
        public static Vector2 PointOnEllipse(Vector2 origin, float radianStep, float width, float height) {
            return new Vector2(
                origin.x + (width * 0.5f) * Mathf.Cos(radianStep),
                origin.y + (height * 0.5f) * Mathf.Sin(radianStep)
            );
        }
        
        public static bool IsWindingClockWise(LightList<Vector2> points) {
            if (points == null || points.Count < 2) {
                return false;
            }
            
            int pointsCount = points.Count;
            Vector2 lastPoint = points[0];
            Vector2[] array = points.Array;
            float sum = 0f;
            
            for (int i = 1; i < pointsCount; i++) {
                sum += (array[i].x - lastPoint.x) * (array[i].y + lastPoint.y);
                lastPoint = array[i];
            }

            return sum >= 0;
        }

        public static bool IsWindingClockWise(Vector2[] points) {
            
            if (points == null || points.Length < 2) {
                return false;
            }
            int pointsCount = points.Length;
            Vector2 lastPoint = points[0];
            float sum = 0f;
            for (int i = 1; i < pointsCount; i++) {
                sum += (points[i].x - lastPoint.x) * (points[i].y + lastPoint.y);
                lastPoint = points[i];
            }

            return sum >= 0;
        }

    }

}