using UnityEngine;

namespace SVGX {

    public struct SVGXClipGroup {

        public int id;
        public int parent;  
        public RangeInt shapeRange;
        public SVGXBounds bounds;

        public SVGXBounds GetBounds(ImmediateRenderContext ctx) {
//
//            SVGXBounds bounds = ctx.GetBounds(shapeRange);
//            
//            if (parent != -1) {
//                return Combine(bounds, ctx.GetClipGroup(parent));
//            }
//            else {
//                return bounds;
//            }
            return default;
        }

        public static SVGXBounds Combine(SVGXBounds a, SVGXBounds b) {
            return new SVGXBounds();
        }

    }

}