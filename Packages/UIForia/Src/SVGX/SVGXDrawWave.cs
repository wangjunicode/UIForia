using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace SVGX {

    internal class SVGXDrawWave {
        
        public LightList<SVGXRenderShape> opaqueFills;
        public LightList<SVGXRenderShape> opaqueStrokes;
                
        public LightList<SVGXStyle> styles;
        public LightList<SVGXMatrix> matrices;
        public readonly LightList<SVGXShape> clipShapes;

        public Mesh clipMesh;

        public SVGXDrawWave() {
            opaqueFills = new LightList<SVGXRenderShape>(32);
            opaqueStrokes = new LightList<SVGXRenderShape>(32);
            matrices = new LightList<SVGXMatrix>();
            styles = new LightList<SVGXStyle>();
            clipShapes = new LightList<SVGXShape>();
        }

        public void AddDrawCall(ImmediateRenderContext ctx, int drawCallIdx, SVGXDrawCall drawCall) {
            LightList<SVGXRenderShape> shapes = null;
            switch (drawCall.type) {
                case DrawCallType.StandardStroke: {
                    shapes = opaqueStrokes;
                    break;
                }

                case DrawCallType.StandardFill: {
                    shapes = opaqueFills;
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
                if (ctx.shapes[i].type != SVGXShapeType.Unset) {
                    shapes.Add(new SVGXRenderShape(ctx.shapes[i], BitUtil.SetHighLowBits(ctx.drawCalls.Count - drawCallIdx, ctx.shapes.Count - i), styleId, matrixId));
                }
            }
            
        }

        public void Clear() {
            styles.Clear();
            matrices.Clear();
            opaqueFills.Clear();
            opaqueStrokes.Clear();
            clipShapes.Clear();
            clipMesh = null;
        }

    }

}