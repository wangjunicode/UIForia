using UIForia.Util;
using UnityEngine;

namespace SVGX {

    internal struct SVGXDrawCall {

        public readonly DrawCallType type;
        public readonly SVGXStyle style;
        public readonly SVGXMatrix matrix;
        public readonly RangeInt shapeRange;

        public SVGXDrawCall(DrawCallType type, SVGXStyle style, SVGXMatrix matrix, RangeInt shapeRange) {
            this.type = type;
            this.style = style;
            this.shapeRange = shapeRange;
            this.matrix = matrix;
        }

        public bool IsTransparentFill => false;//style.fillColor.a < 1;
        public bool IsTransparentStroke => style.strokeStyle.color.a < 1;

    }

}