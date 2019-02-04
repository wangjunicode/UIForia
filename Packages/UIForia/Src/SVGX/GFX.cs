using System;
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

        private Dictionary<SVGXGradient, int> gradientMap = new Dictionary<SVGXGradient, int>();
        private Texture2D gradientAtlas;
        private Color32[] gradientAtlasContents;
        private static readonly int s_GlobalGradientAtlas = Shader.PropertyToID("_globalGradientAtlas");
        private Stack<int> freeGradientRows;
        
        private const int GradientPrecision = 128;

        public GFX(Camera camera) {
            this.camera = camera;
            wavePool = new ObjectPool<SVGXDrawWave>(null, (wave) => wave.Clear());
            gradientAtlas = new Texture2D(GradientPrecision, 32);
            gradientAtlasContents = new Color32[GradientPrecision * gradientAtlas.height];
            freeGradientRows = new Stack<int>(gradientAtlas.height);
            for (int i = 0; i < gradientAtlas.height; i++) {
                freeGradientRows.Push(i);
            }
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
//                Material cutout = stencilStrokeOpaqueCutoutMaterial;
//                Material paint = stencilStrokeOpaquePaintMaterial;
//                Material clear = stencilStrokeOpaqueClearMaterial;
//
//                if (transparent) {
//                    cutout = stencilStrokeTransparentCutoutMaterial;
//                    paint = stencilStrokeTransparentPaintMaterial;
//                    clear = stencilStrokeTransparentClearMaterial;
//                }
//
//                DrawMesh(mesh, cameraMatrix, cutout);
//                DrawMesh(mesh, cameraMatrix, paint);
//                DrawMesh(mesh, cameraMatrix, clear);
            }
            else {
                if (transparent) {
                //    DrawMesh(mesh, cameraMatrix, simpleStrokeTransparentMaterial);
                }
                else {
                    DrawMesh(mesh, cameraMatrix, simpleStrokeOpaqueMaterial);
                }
            }
        }
        
        private void Fill(SVGXDrawWave wave, List<SVGXRenderShape> shapes, bool transparent, bool stencil) {
            // todo -- pool this shit
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

        private void WriteGradient(SVGXGradient gradient, int row) {
            int baseIdx = gradientAtlas.width * row;
            for (int i = 0; i < GradientPrecision; i++) {
                gradientAtlasContents[gradientAtlas.width * row + i] = gradient.Evaluate(i /(float) GradientPrecision);
//                gradientAtlasContents[(gradientAtlas.width * (row + 1)) + i] = gradient.Evaluate(i /(float) GradientPrecision);
//                gradientAtlasContents[(gradientAtlas.width * (row + 2)) + i] = gradient.Evaluate(i /(float) GradientPrecision);
//                gradientAtlasContents[(gradientAtlas.width * (row + 3)) + i] = gradient.Evaluate(i /(float) GradientPrecision);
//                gradientAtlasContents[(gradientAtlas.width * (row + 4)) + i] = gradient.Evaluate(i /(float) GradientPrecision);
            }
            
        }

        private void ExpandGradientTexture() {
            Texture2D newAtlas = new Texture2D(GradientPrecision, gradientAtlas.height * 2);
            UnityEngine.Object.Destroy(gradientAtlas);
            gradientAtlas = newAtlas;
            Array.Resize(ref gradientAtlasContents, GradientPrecision * newAtlas.height);
        }
        
        public void Render(ImmediateRenderContext ctx) {
            this.ctx = ctx;
            SVGXDrawWave wave = CreateWaves(ctx);
            gradientMap.Clear();
            
            // for now assume all uses of a gradient get their own entry in gradient map
            // this means we are ok with duplicated values for now. 
            
            for (int i = 0; i < ctx.gradients.Count; i++) {
                
                if (!gradientMap.ContainsKey(ctx.gradients[i])) {
                    if (gradientMap.Count == gradientAtlas.height) {
                        ExpandGradientTexture();
                    }

                    WriteGradient(ctx.gradients[i], gradientMap.Count);
                    gradientMap.Add(ctx.gradients[i], gradientMap.Count);
                }

            }

            if (ctx.gradients.Count > 0) {
                gradientAtlas.SetPixels32(gradientAtlasContents);
                gradientAtlas.Apply(false);
                Shader.SetGlobalTexture(s_GlobalGradientAtlas, gradientAtlas);
            }

            DrawSimpleOpaqueFill(wave);
            // DrawStencilOpaqueFill(wave);            
            // DrawSimpleOpaqueStroke(wave);
            
            wavePool.Release(wave);
            
        }

    }

}