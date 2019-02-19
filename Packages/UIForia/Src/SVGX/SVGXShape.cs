using UnityEngine;

namespace SVGX {

    internal struct SVGXShape {

        public SVGXShapeType type; 
        public RangeInt pointRange;
        public SVGXBounds bounds;

        public bool isClosed;
        
        public SVGXShape(SVGXShapeType type, RangeInt pointRange, SVGXBounds bounds = default, bool isClosed = false) {
            this.type = type;
            this.pointRange = pointRange;
            this.isClosed = isClosed;
            this.bounds = bounds;
        }

        public Vector4 Dimensions => new Vector4(bounds.min.x, bounds.min.y, bounds.Width, bounds.Height);
        
        public bool RequiresTransparentRendering => type == SVGXShapeType.Path;

    }

}