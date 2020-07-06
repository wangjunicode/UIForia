using System;
using ThisOtherThing.UI.ShapeUtils;
using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI {

    public class GeoUtils {

        [Serializable]
        public class SnappedPositionAndOrientationProperties {

            public enum OrientationTypes {

                Horizontal,
                Vertical

            }

            public enum PositionTypes {

                Center,
                Top,
                Bottom,
                Left,
                Right

            }

            public OrientationTypes Orientation = OrientationTypes.Horizontal;
            public PositionTypes Position = PositionTypes.Center;

        }

        public static readonly float3 UpV3 = Vector3.up;
        public static readonly float3 DownV3 = Vector3.down;
        public static readonly float3 LeftV3 = Vector3.left;
        public static readonly float3 RightV3 = Vector3.right;

        public static readonly float3 ZeroV3 = Vector3.zero;

        public const float HalfPI = Mathf.PI * 0.5f;
        public const float TwoPI = Mathf.PI * 2.0f;

        public static float GetAdjustedAntiAliasing(Canvas canvas, float antiAliasing) {
            return antiAliasing * (1.0f / canvas.scaleFactor);
        }

        public static void AddOffset(ref float width, ref float height, float offset) {
            width += offset * 2.0f;
            height += offset * 2.0f;
        }

        
        public static void SetRadius(ref float2 radius, float width, float height, EllipseProperties properties) {
            width *= 0.5f;
            height *= 0.5f;

            switch (properties.fitting) {
                case EllipseFitting.UniformInner:
                    radius.x = Mathf.Min(width, height);
                    radius.y = radius.x;
                    break;

                case EllipseFitting.UniformOuter:
                    radius.x = Mathf.Max(width, height);
                    radius.y = radius.x;
                    break;

                case EllipseFitting.Ellipse:
                    radius.x = width;
                    radius.y = height;
                    break;
            }
        }

        public static float RadianAngleDifference(float angle1, float angle2) {
            float diff = (angle2 - angle1 + Mathf.PI) % TwoPI - Mathf.PI;
            return diff < -Mathf.PI ? diff + TwoPI : diff;
        }

        public static int SimpleMap(int x, int in_max, int out_max) {
            return x * out_max / in_max;
        }

        public static float SimpleMap(float x, float in_max, float out_max) {
            return x * out_max / in_max;
        }

        public static float Map(float x, float in_min, float in_max, float out_min, float out_max) {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

    }

    public struct UnitPositionData {

        public Vector3[] UnitPositions;

        public float LastBaseAngle;
        public float LastDirection;

    }

}