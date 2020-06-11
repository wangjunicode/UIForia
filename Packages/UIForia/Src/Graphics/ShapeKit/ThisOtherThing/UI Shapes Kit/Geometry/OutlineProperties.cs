using UnityEngine;

namespace ThisOtherThing.UI {

    public struct OutlineProperties {

        public LineType type;
        public float weight;

        public OutlineProperties(LineType lineType, float weight) {
            this.type = lineType;
            this.weight = weight < 0 ? 0 : weight;
        }

        public OutlineProperties(float weight) {
            this.type = LineType.Center;
            this.weight = weight < 0 ? 0 : weight;
        }
        


    }

}