using UnityEngine;

namespace SVGX {

    public struct ColorStop {

        public readonly float time;
        public readonly Color32 color;

        public ColorStop(float time, Color32 color) {
            this.time = time;
            this.color = color;
        }

    }

}