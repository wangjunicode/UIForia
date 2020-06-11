using UnityEngine;

namespace ThisOtherThing.UI {

    public struct AntiAliasingProperties {

        public float AntiAliasing;

        public float Adjusted { get; private set; }

        public void UpdateAdjusted(float scaleFactor) {
            AntiAliasing = Mathf.Max(AntiAliasing, 0.0f);

            if (scaleFactor > 0) {
                Adjusted = AntiAliasing * (1.0f / scaleFactor);
            }
            else {
                Adjusted = AntiAliasing;
            }
        }

    }

}