using Shapes2D;
using UnityEngine;

namespace Src.Rendering {

    public struct Gradient {

        public float start;
        public float rotation;
            
        public Color color0;
        public Color color1;
            
        public Vector2 offset;
            
        public GradientAxis axis;
        public GradientType type;

    }

}