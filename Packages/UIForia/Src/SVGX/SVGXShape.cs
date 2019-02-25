using UnityEngine;

namespace SVGX {

    internal struct SVGXShape {

        public SVGXShapeType type; 
        public RangeInt pointRange;
        public SVGXBounds bounds;

        // can combine type, closed, and textInfoId into 1 int
        public bool isClosed;
        public int textInfoId;
        
        public SVGXShape(SVGXShapeType type, RangeInt pointRange, SVGXBounds bounds = default, bool isClosed = false, int textInfoId = -1) {
            this.type = type;
            this.pointRange = pointRange;
            this.isClosed = isClosed;
            this.bounds = bounds; // untransformed 
            this.textInfoId = textInfoId;
        }

        public Vector4 Dimensions => new Vector4(bounds.min.x, bounds.min.y, bounds.Width, bounds.Height);
        
        public bool RequiresTransparentRendering => type == SVGXShapeType.Path;

    }

   
}