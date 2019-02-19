using System;
using UnityEngine;

namespace SVGX {

    [Serializable]
    public struct ColorStop {

        public float time;
        public Color32 color;

        public ColorStop(float time, Color32 color) {
            this.time = time;
            this.color = color;
        }

    }

}