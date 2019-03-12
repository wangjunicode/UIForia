using UnityEngine;

namespace SVGX {

    internal struct SVGXDrawCall {

        public readonly DrawCallType type;
        public readonly int clipGroupId;
        public readonly SVGXStyle style;
        public readonly SVGXMatrix matrix;
        public readonly RangeInt shapeRange;
        public readonly Rect scissorRect;
        
        public SVGXDrawCall(DrawCallType type, int clipGroupId, SVGXStyle style, Rect scissorRect, SVGXMatrix matrix, RangeInt shapeRange) {
            this.type = type;
            this.style = style;
            this.clipGroupId = clipGroupId;
            this.shapeRange = shapeRange;
            this.matrix = matrix;
            this.scissorRect = scissorRect;
        }

    }

}