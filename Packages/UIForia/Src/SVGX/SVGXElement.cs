using System.Collections.Generic;

namespace SVGX {

    public class SVGXElement {

        public string id;
        public SVGXStyle style;
        public SVGXTransform transform;   
        public SVGXElement parent;

        protected SVGXElement(string id, SVGXStyle style, SVGXTransform transform) {
            this.id = id;
            this.style = style;
            this.transform = transform;
        }

    }

}