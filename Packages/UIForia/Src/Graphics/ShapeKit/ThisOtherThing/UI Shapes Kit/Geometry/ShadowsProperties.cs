using Unity.Mathematics;
using UnityEngine;

namespace ThisOtherThing.UI {

    public struct ShadowsProperties {

        public bool ShowShape;
        public bool ShowShadows;

        public float Angle; // [-1, 1]
        public float Distance; // clamp 0
        public ShadowDescription[] Shadows;
        public Vector2 Offset;

        public static ShadowsProperties CreateDefault() {
            return new ShadowsProperties() {
                ShowShape = true,
                ShowShadows = true,
            };
        }

        public bool ShadowsEnabled {
            get { return ShowShadows && Shadows != null && Shadows.Length > 0; }
        }

        public void UpdateAdjusted() {
            math.sincos(Angle * Mathf.PI - Mathf.PI, out float s, out float c);
            Offset.x = s * Distance;
            Offset.y = c * Distance;
        }

        public Vector2 GetCenterOffset(Vector2 center, int index) {
            center.x += Offset.x + Shadows[index].Offset.x;
            center.y += Offset.y + Shadows[index].Offset.y;

            return center;
        }

    }

}