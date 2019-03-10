using UnityEngine;

namespace UIForia.Extensions {

    public static class Vector2Extensions {

        public static Vector2 Rotate(this Vector2 v, Vector2 pivot, float degrees) {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            v.x -= pivot.x;
            v.y -= pivot.y;
            
            float newX = (cos * v.x) - (sin * v.y);
            float newY = (sin * v.x) + (cos * v.y);

            v.x = pivot.x + newX;
            v.y = pivot.y + newY;
            
            return v;
        }

        public static Vector2 Perpendicular(this Vector2 v) {
            return new Vector2(-v.y, v.x);
        }
        
        public static Vector2 Invert(this Vector2 v) {
            return new Vector2(-v.x, -v.y);
        }
        
        public static float Angle(this Vector2 v) {
            return v.y / v.x;
        }
        
    }

}