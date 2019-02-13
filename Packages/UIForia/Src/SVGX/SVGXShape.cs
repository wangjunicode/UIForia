using UnityEngine;

namespace SVGX {

    internal struct SVGXShape {

        public SVGXShapeType type;
        public RangeInt pointRange;
        public SVGXBounds bounds;

        public bool isHole;
        public bool isClosed;

        public SVGXShape(SVGXShapeType type, RangeInt pointRange) {
            this.type = type;
            this.pointRange = pointRange;
            this.isClosed = false;
            this.isHole = false;
            this.bounds = new SVGXBounds();
        }

    }

}