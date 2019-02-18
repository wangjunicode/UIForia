namespace SVGX {

    internal struct SVGXRenderShape {

        public readonly int matrixId;
        public readonly int styleId;
        public readonly SVGXShape shape;
        public readonly int zIndex;
        
        public SVGXRenderShape(SVGXShape shape, int zIndex, int styleId, int matrixId) {
            this.shape = shape;
            this.zIndex = zIndex;
            this.styleId = styleId;
            this.matrixId = matrixId;
        }

    }

}