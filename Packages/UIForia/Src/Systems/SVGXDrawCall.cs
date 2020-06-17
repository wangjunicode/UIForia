using SVGX;
using UnityEngine;

namespace UIForia.Rendering {

    internal struct SVGXDrawCall {

        public int styleIdx;
        public Material material;
        public GeometryRange geometryRange;

        public readonly DrawCallType type;
        public readonly int transformIdx;
        public readonly RangeInt shapeRange;
        public RangeInt objectRange;
        public int renderStateId;

        public SVGXDrawCall(DrawCallType type, int styleIdx, int transformIdx, in RangeInt shapeRange) {
            this.type = type;
            this.renderStateId = -1;
            this.shapeRange = shapeRange;
            this.transformIdx = transformIdx;
            this.styleIdx = styleIdx;
            this.material = null;
            this.geometryRange = default;
            this.objectRange = default;
        }

    }

}