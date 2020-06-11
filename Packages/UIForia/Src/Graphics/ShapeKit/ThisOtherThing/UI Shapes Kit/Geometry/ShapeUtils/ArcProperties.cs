using System;
using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public struct ArcProperties {

        public ArcDirection direction;
        public float length;
        public float baseAngle;
        public RoundingResolution resolution;

        private int FullCircleResolution;
        private float BaseAngle;

        public static ArcProperties CreateDefault() {
            return new ArcProperties() {
                direction = ArcDirection.Forward,
                length = 1f
            };
        }
        //
        // public int AdjustedResolution { get; private set; }
        // public float AdjustedBaseAngle { get; private set; }
        // public float AdjustedDirection { get; private set; }
        // public float SegmentAngle { get; private set; }
        //
        // public float EndSegmentAngle { get; private set; }
        //
        // private Vector3 centerNormal;
        // private Vector3 startTangent;
        // private Vector3 endTangent;
        // private Vector3 endSegmentUnitPosition;
        //
        // public Vector3 EndSegmentUnitPosition {
        //     get { return endSegmentUnitPosition; }
        // }
        //
        // public Vector3 StartTangent {
        //     get { return startTangent; }
        // }
        //
        // public Vector3 EndTangent {
        //     get { return endTangent; }
        // }
        //
        // public Vector3 CenterNormal {
        //     get { return centerNormal; }
        // }

        public void UpdateAdjusted(int FullCircleResolution, float BaseAngle) {
            // switch (direction) {
            //     case ArcDirection.Backward:
            //         AdjustedDirection = -1.0f;
            //         break;
            //
            //     case ArcDirection.Centered:
            //         AdjustedDirection = 1.0f;
            //         BaseAngle -= length;
            //         break;
            //
            //     case ArcDirection.Forward:
            //         AdjustedDirection = 1.0f;
            //         break;
            //
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }

            // AdjustedResolution = Mathf.CeilToInt(FullCircleResolution * length);
            // AdjustedBaseAngle = BaseAngle * Mathf.PI;
            //
            // SegmentAngle = (Mathf.PI * 2.0f) / AdjustedResolution;
            //
            // EndSegmentAngle = AdjustedBaseAngle + ((Mathf.PI * 2.0f) * length) * AdjustedDirection;
            //
            // endSegmentUnitPosition.x = Mathf.Sin(EndSegmentAngle);
            // endSegmentUnitPosition.y = Mathf.Cos(EndSegmentAngle);
            //
            // endTangent.x = endSegmentUnitPosition.y * AdjustedDirection;
            // endTangent.y = endSegmentUnitPosition.x * -AdjustedDirection;
            //
            // startTangent.x = Mathf.Cos(AdjustedBaseAngle) * -AdjustedDirection;
            // startTangent.y = Mathf.Sin(AdjustedBaseAngle) * AdjustedDirection;
            //
            // float centerAngle = AdjustedBaseAngle + (Mathf.PI * length) * AdjustedDirection;
            // float lengthScaler = 1.0f / Mathf.Sin(Mathf.PI * length);
            // lengthScaler = Mathf.Min(4.0f, lengthScaler);
            // centerNormal.x = -Mathf.Sin(centerAngle) * lengthScaler;
            // centerNormal.y = -Mathf.Cos(centerAngle) * lengthScaler;
        }

    }

}