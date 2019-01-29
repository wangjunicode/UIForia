using UnityEngine;

namespace SVGX {

    internal struct SVGXDrawCall {

        public readonly DrawCallType type;
        public readonly SVGXStyle style;
        public readonly RangeInt shapeRange;
        
        public SVGXDrawCall(DrawCallType type, SVGXStyle style, RangeInt shapeRange) {
            this.type = type;
            this.style = style;
            this.shapeRange = shapeRange;
        }

    }

}