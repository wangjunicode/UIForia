using UIForia.Rendering;
using UnityEngine;

namespace SVGX {

    internal struct SVGXDrawCall {

        public readonly DrawCallType type;
        public readonly int clipGroupId;
        public readonly SVGXStyle style;
        public readonly SVGXMatrix matrix;
        public readonly RangeInt shapeRange;
        public readonly Rect scissorRect;
        public int styleIdx;
        public Material material;
        public int textureId;
        public GeometryRange geometryRange;

        public SVGXDrawCall(DrawCallType type, int clipGroupId, SVGXStyle style, Rect scissorRect, SVGXMatrix matrix, RangeInt shapeRange) {
            this.type = type;
            this.style = style;
            this.clipGroupId = clipGroupId;
            this.shapeRange = shapeRange;
            this.matrix = matrix;
            this.scissorRect = scissorRect;
            this.styleIdx = 0;
            this.material = null;
            this.textureId = -1;
            this.geometryRange = default;
        }

    }
    
    internal struct SVGXDrawCall2 {

        public int styleIdx;
        public Material material;
        public GeometryRange geometryRange;
        
        public readonly DrawCallType type;
        public readonly int transformIdx;
        public readonly RangeInt shapeRange;
        public RangeInt objectRange;
        public int renderStateId;

        public SVGXDrawCall2(DrawCallType type, int styleIdx, int transformIdx, in RangeInt shapeRange) {
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