using UnityEngine;

namespace ThisOtherThing.UI {

    public struct ShadowDescription {

        public float Size;
        public Color32 Color;
        public Vector2 Offset;
        public float Softness; //[0, 1]

        public static ShadowDescription CreateDefault() {
            return new ShadowDescription() {
                Color = new Color32(0, 0, 0, 120),
                Size = 5f,
                Softness = 0.5f
            };
        }

    }

}