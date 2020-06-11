using System.Runtime.InteropServices;
using ThisOtherThing.UI.ShapeUtils;
using UnityEngine;

namespace ThisOtherThing.UI {

    [StructLayout(LayoutKind.Explicit)]
    public struct RoundingResolution {

        [FieldOffset(0)] public ResolutionType resolutionType;
        [FieldOffset(4)] public float maxDistance;
        [FieldOffset(4)] public int fixedResolution;

    }

    public struct RoundingProperties {

        public RoundingResolution roundingResolution;

        public int AdjustedResolution { private set; get; }
        public bool MakeSharpCorner { private set; get; }

        public void UpdateAdjusted(float radius, float offset, float numCorners) {
            UpdateAdjusted(radius, offset, this, numCorners);
        }

        public void UpdateAdjusted(float radius, float offset, RoundingProperties overrideProperties, float numCorners) {
            MakeSharpCorner = radius < 0.001f;

            radius += offset;

            switch (overrideProperties.roundingResolution.resolutionType) {
                case ResolutionType.Calculated:
                    float circumference = GeoUtils.TwoPI * radius;

                    AdjustedResolution = Mathf.CeilToInt(circumference / overrideProperties.roundingResolution.maxDistance / numCorners);
                    AdjustedResolution = Mathf.Max(AdjustedResolution, 2);
                    break;

                case ResolutionType.Fixed:
                    AdjustedResolution = overrideProperties.roundingResolution.fixedResolution;
                    break;
            }
        }

    }

}