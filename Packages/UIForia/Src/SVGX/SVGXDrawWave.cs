using System.Collections.Generic;
using UIForia.Util;

namespace SVGX {

    internal class SVGXDrawWave {

        public LightList<SVGXRenderShape> transparentFills;
        public LightList<SVGXRenderShape> transparentStrokes;
        
        public LightList<SVGXRenderShape> opaqueFills;
        public LightList<SVGXRenderShape> opaqueStrokes;
                
        public LightList<SVGXStyle> styles;
        public LightList<SVGXMatrix> matrices;
        
        private bool opaqueNeedsStencilFill;
        private bool transparentNeedsStencilFill;
        
        public SVGXDrawWave() {
            transparentFills = new LightList<SVGXRenderShape>(32);
            transparentStrokes = new LightList<SVGXRenderShape>(32);
            opaqueFills = new LightList<SVGXRenderShape>(32);
            opaqueStrokes = new LightList<SVGXRenderShape>(32);
            matrices = new LightList<SVGXMatrix>();
            styles = new LightList<SVGXStyle>();
        }

        public void AddDrawCall(ImmediateRenderContext ctx, SVGXDrawCall drawCall) {
            LightList<SVGXRenderShape> shapes = null;
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