using System.Collections.Generic;

namespace SVGX {

    public class SVGXGroup : SVGXElement {

        public readonly List<SVGXElement> children;
        
        public SVGXGroup(string id, SVGXStyle style, SVGXTransform transform, List<SVGXElement> children) : base(id, style, transform) {
            children = children ?? new List<SVGXElement>();
        }
        
    }

}