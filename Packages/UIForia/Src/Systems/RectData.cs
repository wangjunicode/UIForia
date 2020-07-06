using ThisOtherThing.UI;
using ThisOtherThing.UI.ShapeUtils;
using UIForia.Graphics;
using UnityEngine;

namespace UIForia.Systems {

    public struct RectData {

        public ShapeMode type;
        public float x;
        public float y;
        public float width;
        public float height;
        public Color32 color;
        public EdgeGradientData edgeGradient;

    }

    public struct RoundedRectData {

        public ShapeMode type;
        public float x;
        public float y;
        public float width;
        public float height;
        public Color32 color;
        public CornerProperties cornerProperties;
        public EdgeGradientData edgeGradient;

    }

}