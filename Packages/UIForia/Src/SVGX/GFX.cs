using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

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

    public class GFX {

        public Camera camera;
        public ImmediateRenderContext ctx;

        private ObjectPool<SVGXDrawWave> wavePool;
        internal Material stencilFillOpaqueCutoutMaterial;
        internal Material stencilFillOpaquePaintMaterial;
        internal Material stencilFillOpaqueClearMaterial;

        internal Material stencilFillTransparentCutoutMaterial;
        internal Material stencilFillTransparentPaintMaterial;
        internal Material stencilFillTransparentClearMaterial;

        internal Material simpleFillOpaqueMaterial;
        internal Material simpleFillTransparentMaterial;
        
        internal Material simpleStrokeOpaqueMaterial;
        internal Material simpleStrokeTransparentMaterial;

        public GFX(Camera camera) {
            this.camera = camera;
            wavePool = new ObjectPool<SVGXDrawWave>(null, (wave) => wave.Clear());
        }

        public void DrawMesh(Mesh mesh, Matrix4x4 transform, Material material) {
            Graphics.DrawMesh(mesh, transform, material, 0, camera, 0, null, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
        }

        internal SVGXDrawWave CreateWaves(ImmediateRenderContext ctx) {
            // get clip groups ignoring hierarchy
            // each clip group becomes a wave
            // waves get merged if they have non overlapping bounds
            // different draw calls can contain the same shapes, probably can't totally re-use geometry because of changes to transform
            // or require 2nd pass before rendering that does the transform 

            SVGXDrawWave wave = wavePool.Get();

            for (int i = 0; i < ctx.drawCalls.Count; i++) {
                wave.AddDrawCall(ctx, ctx.drawCalls[i]);
            }

            return wave;
        }

        private void Stroke(SVGXDrawWave wave, List<SVGXRenderShape> shapes, bool transparent, bool stencil) {
            StrokeVertexData strokeVertexData = new StrokeVertexData();
            
            for (int i = 0; i < shapes.Count; i++) {
                SVGXRenderShape shape = shapes[i];
                SVGXStyle style = wave.styles[shape.styleId];
                SVGXMatrix matrix = wave.matrices[shape.matrixId];
                SVGXRenderSystem.CreateSolidStrokeVertices(strokeVertexData, ctx.points, matrix, style, shapes[i].shape);
            }
            
            Mesh mesh = strokeVertexData.FillMesh();
            
            Matrix4x4 cameraMatrix = Matrix4x4.TRS(camera.transform.position + new Vector3(0, 0, 2), Quaternion.identity, Vector3.one);
            
            if (stencil) {
//                Material cutout = stencilStrokelOpaqueCutoutMaterial;
//                Material paint = stencilStrokelOpaquePaintMaterial;
//                Material clear = stencilStrokelOpaqueClearMaterial;
//
//                if (transparent) {
//                    cutout = stencilStrokelTransparentCutoutMaterial;
//                    paint = stencilStrokelTransparentPaintMaterial;
//                    clear = stencilStrokelTransparentClearMaterial;
//                }
//
//                DrawMesh(mesh, cameraMatrix, cutout);
//                DrawMesh(mesh, cameraMatrix, paint);
//                DrawMesh(mesh, cameraMatrix, clear);
            }
            else {
                if (transparent) {
                //    DrawMesh(mesh, cameraMatrix, simpleStrokelTransparentMaterial);
                }
                else {
                    DrawMesh(mesh, cameraMatrix, simpleStrokeOpaqueMaterial);
                }
            }
        }
        
        private void Fill(SVGXDrawWave wave, List<SVGXRenderShape> shapes, bool transparent, bool stencil) {
            FillVertexData fillVertexData = new FillVertexData();

            for (int i = 0; i < shapes.Count; i++) {
                SVGXRenderShape shape = shapes[i];
                SVGXStyle style = wave.styles[shape.styleId];
                SVGXMatrix matrix = wave.matrices[shape.matrixId];
                SVGXRenderSystem.CreateFillVertices(fillVertexData, ctx.points, matrix, style, shapes[i].shape);
            }

            Mesh mesh = fillVertexData.FillMesh();

            Matrix4x4 cameraMatrix = Matrix4x4.TRS(camera.transform.position + new Vector3(0, 0, 2), Quaternion.identity, Vector3.one);
            
            if (stencil) {
                Material cutout = stencilFillOpaqueCutoutMaterial;
                Material paint = stencilFillOpaquePaintMaterial;
                Material clear = stencilFillOpaqueClearMaterial;

                if (transparent) {
                    cutout = stencilFillTransparentCutoutMaterial;
                    paint = stencilFillTransparentPaintMaterial;
                    clear = stencilFillTransparentClearMaterial;
                }

                DrawMesh(mesh, cameraMatrix, cutout);
                DrawMesh(mesh, cameraMatrix, paint);
                DrawMesh(mesh, cameraMatrix, clear);
            }
            else {
                if (transparent) {
                    DrawMesh(mesh, cameraMatrix, simpleFillTransparentMaterial);
                }
                else {
                    DrawMesh(mesh, cameraMatrix, simpleFillOpaqueMaterial);
                }
            }
        }

        internal void StencilFill(SVGXDrawWave wave, List<SVGXRenderShape> shapes, bool transparent) {
            Fill(wave, shapes, transparent, true);
        }

        internal void SimpleFill(SVGXDrawWave wave, List<SVGXRenderShape> shapes, bool transparent) {
            Fill(wave, shapes, transparent, false);
        }

        private void DrawSimpleOpaqueFill(SVGXDrawWave wave) {
            Fill(wave, wave.opaqueFills, false, false);
        }
        
        private void DrawStencilOpaqueFill(SVGXDrawWave wave) {
            Fill(wave, wave.opaqueFills, false, true);
        }

        private void DrawSimpleOpaqueStroke(SVGXDrawWave wave) {
            Stroke(wave, wave.opaqueStrokes, false, false);
        }
        
        public void Render(ImmediateRenderContext ctx) {
            this.ctx = ctx;
            SVGXDrawWave wave = CreateWaves(ctx);
            
            DrawSimpleOpaqueFill(wave);
            DrawSimpleOpaqueStroke(wave);
//            DrawStencilOpaqueFill(wave);
            
            wavePool.Release(wave);
            
        }

    }

}