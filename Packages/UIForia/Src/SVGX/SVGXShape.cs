using UnityEngine;

namespace SVGX {

    internal struct SVGXShape {

        public SVGXShapeType type;
        public RangeInt pointRange;
        public SVGXBounds bounds;

        public bool isClosed;
        public Vector3 origin;
        
        public SVGXShape(SVGXShapeType type, RangeInt pointRange, Vector3 origin = default) {
            this.type = type;
            this.pointRange = pointRange;
            this.isClosed = false;
            this.bounds = new SVGXBounds();
            this.origin = origin;
        }

        public Vector4 Dimensions => new Vector4(origin.x, origin.y, bounds.Width, bounds.Height);

    }

}