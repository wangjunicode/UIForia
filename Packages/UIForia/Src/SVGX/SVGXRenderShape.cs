using UIForia.Text;

namespace SVGX {

    internal struct SVGXRenderShape {

        public readonly int matrixId;
        public readonly int styleId;
        public readonly SVGXShape shape;
        public readonly int zIndex;
        public readonly DrawCallType drawCallType;
        public readonly TextInfo textInfo;
        
        public SVGXRenderShape(SVGXShape shape, int zIndex, int styleId, int matrixId, DrawCallType drawCallType, TextInfo textInfo = null) {
            this.shape = shape;
            this.zIndex = zIndex;
            this.styleId = styleId;
            this.matrixId = matrixId;
            this.drawCallType = drawCallType;
            this.textInfo = textInfo;
        }

    }

}