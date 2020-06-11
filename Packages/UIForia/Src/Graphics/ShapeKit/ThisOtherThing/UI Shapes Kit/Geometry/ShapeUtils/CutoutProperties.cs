using UnityEngine;

namespace ThisOtherThing.UI.ShapeUtils {

    public struct CutoutProperties {

        public int Resolution; // default 4, min 3
        public float Radius; //= 1.0f;

        [Range(-3.141592f, 3.141592f)] // [-pi, pi]
        public float RotationOffset;

        public UnitPositionData UnitPositionData;

    }

}