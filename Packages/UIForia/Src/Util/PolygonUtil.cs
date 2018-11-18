using UnityEngine;

namespace UIForia.Src.Util {

    public static class PolygonUtil {

        public static bool PointInPolygon(Vector2 point, Vector2[] polygon) {
            int indexer = polygon.Length - 1;
            bool inside = false;
            float x = point.x;
            float y = point.y;
            for (int i = 0; i < polygon.Length; indexer = i++) {
                float polyX = polygon[i].x;
                float polyY = polygon[i].y;
                inside ^= polyY > y ^ polygon[indexer].y > y && 
                          x < (polygon[indexer].x - polyX)
                          * (y - polyY)
                          / (polygon[indexer].y - polyY)
                          + polyX;
            }
            return inside;
        }

    }

}