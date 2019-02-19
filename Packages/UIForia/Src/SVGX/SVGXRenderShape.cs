namespace SVGX {

    internal struct SVGXRenderShape {

        public readonly int matrixId;
        public readonly int styleId;
        public readonly SVGXShape shape;
        public readonly int zIndex;
        public readonly DrawCallType drawCallType;
        
        public SVGXRenderShape(SVGXShape shape, int zIndex, int styleId, int matrixId, DrawCallType drawCallType) {
            this.shape = shape;
            this.zIndex = zIndex;
            this.styleId = styleId;
            this.matrixId = matrixId;
            this.drawCallType = drawCallType;
        }

    }

}