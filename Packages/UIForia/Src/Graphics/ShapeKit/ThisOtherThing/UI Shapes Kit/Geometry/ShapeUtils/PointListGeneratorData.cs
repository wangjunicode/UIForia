using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public struct PointListGeneratorData {

        public PointListGeneratorType pointListGeneratorType;

        public Vector2 center;

        public float width;
        public float height;
        public float radius;

        public float direction;

        public float[] FloatValues;
        public float minValue;
        public float maxValue;

        public float startOffset;
        public float length;
        public float endRadius;
        public int resolution;

        public bool centerPoint;
        public bool skipLastPosition;

        public float angle;
        public float innerScale; // = 0.8f;
        public float outerScale; // = 0.5f;

    }

}