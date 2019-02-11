namespace SVGX {

    internal struct SVGXRenderShape {

        public readonly int matrixId;
        public readonly int styleId;
        public readonly SVGXShape shape;

        public SVGXRenderShape(SVGXShape shape, int styleId, int matrixId) {
            this.shape = shape;
            this.styleId = styleId;
            this.matrixId = matrixId;
        }

    }

}