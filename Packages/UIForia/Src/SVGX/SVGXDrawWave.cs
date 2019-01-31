using System.Collections.Generic;

namespace SVGX {

    internal class SVGXDrawWave {

        public List<SVGXRenderShape> transparentFills;
        public List<SVGXRenderShape> transparentStrokes;
        
        public List<SVGXRenderShape> opaqueFills;
        public List<SVGXRenderShape> opaqueStrokes;
                
        public List<SVGXStyle> styles;
        public List<SVGXMatrix> matrices;
        
        private bool opaqueNeedsStencilFill;
        private bool transparentNeedsStencilFill;
        
        public SVGXDrawWave() {
            transparentFills = new List<SVGXRenderShape>(32);
            transparentStrokes = new List<SVGXRenderShape>(32);
            opaqueFills = new List<SVGXRenderShape>(32);
            opaqueStrokes = new List<SVGXRenderShape>(32);
            matrices = new List<SVGXMatrix>();
            styles = new List<SVGXStyle>();
        }

        public void AddDrawCall(ImmediateRenderContext ctx, SVGXDrawCall drawCall) {
            List<SVGXRenderShape> shapes = null;
            switch (drawCall.type) {
                case DrawCallType.StandardStroke: {
                    shapes = opaqueStrokes;
//                    shapes = drawCall.IsTransparentStroke ? transparentStrokes : opaqueStrokes;
                    break;
                }

                case DrawCallType.StandardFill: {
                    shapes = drawCall.IsTransparentFill ? transparentFills : opaqueFills;
//                    opaqueNeedsStencilFill = opaqueNeedsStencilFill ? opaqueNeedsStencilFill : drawCall.requiresStencilFill;
                    break;
                }
            }

            if (shapes == null) {
                return;
            }

            styles.Add(drawCall.style);
            matrices.Add(drawCall.matrix);
            
            int styleId = styles.Count - 1;
            int matrixId = matrices.Count - 1;
            
            for (int i = drawCall.shapeRange.start; i < drawCall.shapeRange.end; i++) {
                shapes.Add(new SVGXRenderShape(ctx.shapes[i], styleId, matrixId));
            }
            
        }

        public void Clear() {
            styles.Clear();
            matrices.Clear();
            transparentFills.Clear();
            opaqueFills.Clear();
            transparentStrokes.Clear();
            opaqueStrokes.Clear();
            opaqueNeedsStencilFill = false;
            transparentNeedsStencilFill = false;
        }

    }

}