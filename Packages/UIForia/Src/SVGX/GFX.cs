using System;
using System.Collections.Generic;
using TMPro;
using UIForia;
using UIForia.Extensions;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;
using Application = UIForia.Application;

namespace SVGX {

    public class GFX {

        public Camera camera;
        public ImmediateRenderContext ctx;

        private int drawCallCnt = 0;

        private Texture2D gradientAtlas;
        private Color32[] gradientAtlasContents;
        private static readonly int s_GlobalGradientAtlas = Shader.PropertyToID("_globalGradientAtlas");
        private static readonly int s_GlobalGradientAtlasSize = Shader.PropertyToID("_globalGradientAtlasSize");
        private Stack<int> freeGradientRows;

        private const int GradientPrecision = 64;

        private static readonly ObjectPool<SVGXDrawWave> s_WavePool;
        private static readonly ObjectPool<FillVertexData> s_FillVertexDataPool;
        private static readonly ObjectPool<StrokeVertexData> s_StrokeVertexDataPool;
        private static readonly Dictionary<SVGXGradient, int> s_GradientRowMap;
        private static readonly IntMap<SVGXGradient> s_GradientMap;
        private static readonly MaterialPool s_SimpleFillPool;

        private readonly LightList<TexturedShapeGroup> texturedShapeGroups;
        private readonly IntMap<Texture2D> textureMap;

        private readonly MaterialPool batchedTransparentPool;
        private readonly DeferredReleasePool<BatchedVertexData> vertexDataPool;

        private readonly LightList<Vector2> scratchPointList;

        private static readonly int s_StencilRefKey = Shader.PropertyToID("_StencilRef");
        private static readonly int s_MainTexKey = Shader.PropertyToID("_MainTex");
        private static readonly int s_GlobalFontTextureKey = Shader.PropertyToID("_globalFontTexture");
        private static readonly int s_GlobalFontData1Key = Shader.PropertyToID("_globalFontData1");
        private static readonly int s_GlobalFontData2Key = Shader.PropertyToID("_globalFontData2");

        static GFX() {
            s_GradientRowMap = new Dictionary<SVGXGradient, int>();
            s_GradientMap = new IntMap<SVGXGradient>();
            s_WavePool = new ObjectPool<SVGXDrawWave>(null, (wave) => wave.Clear());
            s_FillVertexDataPool = new ObjectPool<FillVertexData>((a) => a.Clear());
            s_StrokeVertexDataPool = new ObjectPool<StrokeVertexData>((a) => a.Clear());
        }

        public GFX(Camera camera) {
            this.camera = camera;

            scratchPointList = new LightList<Vector2>(64);
            vertexDataPool = new DeferredReleasePool<BatchedVertexData>();
            
            batchedTransparentPool = new MaterialPool(Application.Settings.svgxMaterial);
            
            gradientAtlas = new Texture2D(GradientPrecision, 32);
            gradientAtlas.wrapMode = TextureWrapMode.Clamp;
            gradientAtlasContents = new Color32[GradientPrecision * gradientAtlas.height];
            for (int i = 0; i < GradientPrecision; i++) {
                gradientAtlasContents[i] = new Color32(255, 255, 255, 255);
            }

            freeGradientRows = new Stack<int>(gradientAtlas.height);
            texturedShapeGroups = new LightList<TexturedShapeGroup>();
            textureMap = new IntMap<Texture2D>();
            for (int i = 0; i < gradientAtlas.height; i++) {
                freeGradientRows.Push(i);
            }
        }

        public void SetCamera(Camera camera) {
            this.camera = camera;
        }

        public void DrawMesh(Mesh mesh, Matrix4x4 transform, Material material) {
            material.renderQueue = drawCallCnt++;
            Graphics.DrawMesh(mesh, transform, material, LayerMask.NameToLayer("UI"), camera, 0, null, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
        }

        internal static LightList<SVGXDrawWave> CreateWaves(ImmediateRenderContext ctx) {
            // 1 wave per compound clip group
            // if clip groups do not overlap we can combine two waves            
            // for each draw call
            // get clip id
            // if clip id == -1 -> no clipping
            // while clip id == last clip id
            // add draw call to current wave
            // if clip id is different
            // create new wave

            if (ctx.drawCalls.Count == 0) return null;

            int lastClipId = ctx.drawCalls[0].clipGroupId;
            LightList<SVGXDrawWave> waves = LightList<SVGXDrawWave>.Get();

            SVGXDrawWave wave = s_WavePool.Get();

            if (lastClipId != -1) {
                int clipId = lastClipId;

                while (clipId != -1) {
                    SVGXClipGroup clipGroup = ctx.clipGroups[clipId];
                    int start = clipGroup.shapeRange.start;
                    int end = clipGroup.shapeRange.end;

                    for (int j = start; j < end; j++) {
                        wave.clipShapes.Add(ctx.shapes[j]);
                    }

                    clipId = ctx.clipGroups[clipId].parent;
                }
            }

            for (int i = 0; i < ctx.drawCalls.Count; i++) {
                int clipId = ctx.drawCalls[i].clipGroupId;

                if (clipId != lastClipId) {
                    waves.Add(wave);
                    wave = s_WavePool.Get();
                    lastClipId = clipId;

                    while (clipId != -1) {
                        SVGXClipGroup clipGroup = ctx.clipGroups[clipId];
                        int start = clipGroup.shapeRange.start;
                        int end = clipGroup.shapeRange.end;

                        for (int j = start; j < end; j++) {
                            wave.clipShapes.Add(ctx.shapes[j]);
                        }

                        clipId = ctx.clipGroups[clipId].parent;
                    }
                }

                wave.AddDrawCall(ctx, i, ctx.drawCalls[i]);
            }

            waves.Add(wave);

            // todo -- merge non overlapping waves

            return waves;
        }

        private struct TexturedShapeGroup {

            public int textureId;
            public LightList<SVGXRenderShape> shapes;

        }

        private void CreateClipFillVertices(SVGXDrawWave wave, Vector2[] points) {
            FillVertexData vertexData = s_FillVertexDataPool.Get();

            int triIdx = vertexData.triangleIndex;
            int vertexCnt = vertexData.position.Count;
            int colorCnt = vertexData.colors.Count;
            int texCoordCnt = vertexData.texCoords.Count;
            int flagsCnt = vertexData.flags.Count;
            int triangleCnt = vertexData.triangles.Count;

            Vector3[] vertices = vertexData.position.Array;
            Vector4[] texCoords = vertexData.texCoords.Array;
            Vector4[] flags = vertexData.flags.Array;
            int[] triangles = vertexData.triangles.Array;

            SVGXShape[] shapes = wave.clipShapes.Array;

            for (int i = 0; i < wave.clipShapes.Count; i++) {
                SVGXShape shape = shapes[i];
                int start = shape.pointRange.start;

                switch (shape.type) {
                    case SVGXShapeType.Unset:
                        break;
                    case SVGXShapeType.Ellipse:
                    case SVGXShapeType.Circle:
                    case SVGXShapeType.Rect:

                        vertices[vertexCnt++] = points[start++];
                        vertices[vertexCnt++] = points[start++];
                        vertices[vertexCnt++] = points[start++];
                        vertices[vertexCnt++] = points[start];

                        triangles[triangleCnt++] = triIdx + 0;
                        triangles[triangleCnt++] = triIdx + 1;
                        triangles[triangleCnt++] = triIdx + 2;
                        triangles[triangleCnt++] = triIdx + 2;
                        triangles[triangleCnt++] = triIdx + 3;
                        triangles[triangleCnt++] = triIdx + 0;

                        texCoords[texCoordCnt++] = new Vector2(0, 1);
                        texCoords[texCoordCnt++] = new Vector2(1, 1);
                        texCoords[texCoordCnt++] = new Vector2(1, 0);
                        texCoords[texCoordCnt++] = new Vector2(0, 0);

                        flags[flagsCnt++] = new Vector4((int) shape.type, 0, 0, 0);
                        flags[flagsCnt++] = new Vector4((int) shape.type, 0, 0, 0);
                        flags[flagsCnt++] = new Vector4((int) shape.type, 0, 0, 0);
                        flags[flagsCnt++] = new Vector4((int) shape.type, 0, 0, 0);

                        vertexData.position.Count = vertexCnt;
                        vertexData.triangles.Count = triangleCnt;
                        vertexData.colors.Count = colorCnt;
                        vertexData.texCoords.Count = texCoordCnt;
                        vertexData.flags.Count = flagsCnt;

                        vertexData.triangleIndex = triIdx + 4;

                        break;

                    case SVGXShapeType.Path:
                        Debug.Log("Path");
                        break;
                    case SVGXShapeType.RoundedRect:
                        Debug.Log("Rounded Rect Clip not implemented");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            wave.clipMesh = vertexData.FillMesh();
        }

        private Matrix4x4 OriginMatrix {
            get {
                Vector3 origin = camera.transform.position;
                origin.x -= 0.5f * Screen.width;
                origin.y += 0.5f * Screen.height;
                origin.z += 2;
                return Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
            }
        }

        private void WriteGradient(SVGXGradient gradient, int row) {
            int baseIdx = gradientAtlas.width * row;
            for (int i = 0; i < GradientPrecision; i++) {
                gradientAtlasContents[baseIdx + i] = gradient.Evaluate(i / (float) GradientPrecision);
            }
        }

        private void ExpandGradientTexture() {
            Texture2D newAtlas = new Texture2D(GradientPrecision, gradientAtlas.height * 2);
            UnityEngine.Object.Destroy(gradientAtlas);
            gradientAtlas = newAtlas;
            gradientAtlas.wrapMode = TextureWrapMode.Clamp;
            Array.Resize(ref gradientAtlasContents, GradientPrecision * newAtlas.height);
        }

        public void Render(ImmediateRenderContext ctx) {
            this.ctx = ctx;
            drawCallCnt = 0;

            vertexDataPool.FlushReleaseQueue();
            batchedTransparentPool.FlushReleaseQueue();

            textureMap.Clear();
            s_GradientMap.Clear();
            s_GradientRowMap.Clear();

            // for now assume all uses of a gradient get their own entry in gradient map
            // this means we are ok with duplicated values for now. 

            for (int i = 0; i < ctx.textures.Count; i++) {
                textureMap[ctx.textures[i].GetInstanceID()] = ctx.textures[i];
            }

            for (int i = 0; i < ctx.gradients.Count; i++) {
                SVGXGradient gradient = ctx.gradients[i];
                if (!s_GradientRowMap.ContainsKey(gradient)) {
                    if (s_GradientRowMap.Count == gradientAtlas.height - 1) {
                        ExpandGradientTexture();
                    }

                    WriteGradient(gradient, s_GradientRowMap.Count + 1);
                    s_GradientRowMap.Add(gradient, s_GradientRowMap.Count + 1);
                    s_GradientMap.Add(gradient.id, gradient);
                }
            }

            if (ctx.gradients.Count > 0) {
                gradientAtlas.SetPixels32(gradientAtlasContents);
                gradientAtlas.Apply(false);
                Shader.SetGlobalTexture(s_GlobalGradientAtlas, gradientAtlas);
                Shader.SetGlobalFloat(s_GlobalGradientAtlasSize, gradientAtlas.height);
            }

            LightList<SVGXDrawWave> waves = CreateWaves(ctx);

            if (waves == null) return;

            LightList<SVGXStyle> styles = LightList<SVGXStyle>.Get();
            LightList<SVGXMatrix> matrices = LightList<SVGXMatrix>.Get();
            LightList<Rect> scissors = LightList<Rect>.Get();

            SVGXDrawWave[] waveArray = waves.Array;
            for (int i = 0; i < waves.Count; i++) {
                SVGXDrawWave wave = waveArray[i];

                DrawClip(wave);

                LightList<SVGXRenderShape> transparents = LightList<SVGXRenderShape>.Get();

                // j is the wave's draw call need the over all offset as well
                for (int j = 0; j < wave.drawCalls.Count; j++) {
                    // for each shape in draw call

                    SVGXDrawCall call = wave.drawCalls[j];
                    styles.Add(call.style);
                    matrices.Add(call.matrix);
                    scissors.Add(call.scissorRect);
                    
                    switch (call.type) {
                        case DrawCallType.StandardStroke: {
                            for (int k = call.shapeRange.start; k < call.shapeRange.end; k++) {
                                int z = ushort.MaxValue - k;
                                TextInfo textInfo = null;
                                if (ctx.shapes[k].type == SVGXShapeType.Text) {
                                    textInfo = ctx.textInfos[ctx.shapes[k].textInfoId];
                                }

                                transparents.Add(new SVGXRenderShape(ctx.shapes[k], z, j, j, DrawCallType.StandardStroke, textInfo));
                            }

                            break;
                        }
                        case DrawCallType.StandardFill: {
                            for (int k = call.shapeRange.start; k < call.shapeRange.end; k++) {
                                int z = ushort.MaxValue - k;
                                TextInfo textInfo = null;
                                if (ctx.shapes[k].type == SVGXShapeType.Text) {
                                    textInfo = ctx.textInfos[ctx.shapes[k].textInfoId];
                                }


                                transparents.Add(new SVGXRenderShape(ctx.shapes[k], z, j, j, DrawCallType.StandardFill, textInfo));
                            }

                            break;
                        }
                        case DrawCallType.Shadow:
                            for (int k = call.shapeRange.start; k < call.shapeRange.end; k++) {
                                int z = ushort.MaxValue - k;
                                TextInfo textInfo = null;

                                if (ctx.shapes[k].type == SVGXShapeType.Text) {
                                    textInfo = ctx.textInfos[ctx.shapes[k].textInfoId];
                                }

                                transparents.Add(new SVGXRenderShape(ctx.shapes[k], z, j, j, DrawCallType.Shadow, textInfo));
                            }

                            break;
                    }
                }

                transparents.Sort((a, b) => a.zIndex > b.zIndex ? -1 : 1);

                DrawBatchedTransparents(ctx.points.Array, transparents, scissors, styles, matrices);

                LightList<SVGXRenderShape>.Release(ref transparents);

                ClearClip(wave);

                s_WavePool.Release(wave);
            }

            LightList<SVGXDrawWave>.Release(ref waves);
            LightList<SVGXStyle>.Release(ref styles);
            LightList<SVGXMatrix>.Release(ref matrices);
            LightList<Rect>.Release(ref scissors);
        }

        private static void GroupByTexture(LightList<SVGXStyle> styles, LightList<SVGXRenderShape> shapes, LightList<TexturedShapeGroup> retn) {
            for (int i = 0; i < shapes.Count; i++) {
                SVGXStyle style = styles[shapes[i].styleId];
                if ((style.fillColorMode & ColorMode.Texture) != 0) {
                    int textureId = style.fillTextureId;
                    int idx = retn.FindIndex(textureId, ((group, texId) => group.textureId == texId));
                    if (idx == -1) {
                        TexturedShapeGroup group = new TexturedShapeGroup() {
                            textureId = textureId,
                            shapes = LightList<SVGXRenderShape>.Get()
                        };
                        group.shapes.Add(shapes[i]);
                        retn.Add(group);
                    }
                    else {
                        retn[idx].shapes.Add(shapes[i]);
                    }
                }
                else {
                    int idx = retn.FindIndex(-1, ((group, texId) => group.textureId == texId));
                    if (idx == -1) {
                        TexturedShapeGroup group = new TexturedShapeGroup() {
                            textureId = -1,
                            shapes = LightList<SVGXRenderShape>.Get()
                        };
                        group.shapes.Add(shapes[i]);
                        retn.Add(group);
                    }
                    else {
                        retn[idx].shapes.Add(shapes[i]);
                    }
                }
            }
        }

//        private void DrawBatchedOpaques(Vector2[] points, LightList<SVGXRenderShape> renderShapes, Rect scissorRect, LightList<SVGXStyle> styles, LightList<SVGXMatrix> matrices) {
//            GroupByTexture(styles, renderShapes, texturedShapeGroups);
//            TexturedShapeGroup[] array = texturedShapeGroups.Array;
//
//            Material material = null;
//            Matrix4x4 originMatrix = OriginMatrix;
//            int lastFontId = -1;
//
//            for (int i = 0; i < texturedShapeGroups.Count; i++) {
//                LightList<SVGXRenderShape> shapes = array[i].shapes;
//                BatchedVertexData batchedVertexData = vertexDataPool.GetAndQueueForRelease();
//
//                for (int j = 0; j < shapes.Count; j++) {
//                    SVGXGradient gradient = s_GradientMap.GetOrDefault(styles[shapes[j].styleId].fillGradientId);
//                    GradientData gradientData = default;
//
//                    if (gradient != null) {
//                        int gradientId = s_GradientRowMap.GetOrDefault(gradient);
//                        gradientData = new GradientData(gradientId, gradient);
//                    }
//
//                    if (shapes[j].shape.type == SVGXShapeType.Text) {
//                        TextInfo2 textInfo = shapes[j].textInfo;
//                        int currentFontId = textInfo.spanInfos[0].font.GetInstanceID();
//
//                        if (lastFontId != currentFontId) {
//                            if (lastFontId != -1 && j != 0) {
//                                material = batchedTransparentPool.GetAndQueueForRelease();
//                                material.SetTexture(s_MainTexKey, textureMap.GetOrDefault(array[i].textureId));
//                                DrawMesh(batchedVertexData.FillMesh(), originMatrix, material);
//                                batchedVertexData = vertexDataPool.GetAndQueueForRelease();
//                            }
//
//                            Material fontMaterial = textInfo.spanInfos[0].font.material;
//
//                            Shader.SetGlobalTexture(s_GlobalFontTextureKey, textInfo.spanInfos[0].font.atlas);
//
//                            Shader.SetGlobalVector(s_GlobalFontData1Key, new Vector4(
//                                fontMaterial.GetFloat(ShaderUtilities.ID_WeightNormal),
//                                fontMaterial.GetFloat(ShaderUtilities.ID_WeightBold),
//                                textInfo.spanInfos[0].font.atlas.width,
//                                textInfo.spanInfos[0].font.atlas.height)
//                            );
//
//                            Shader.SetGlobalVector(s_GlobalFontData2Key, new Vector4(
//                                fontMaterial.GetFloat(ShaderUtilities.ID_GradientScale),
//                                fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A),
//                                fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_B),
//                                fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_C))
//                            );
//                        }
//                    }
//
//                    batchedVertexData.CreateFillVertices(points, shapes[j], scissorRect, gradientData, styles[shapes[j].styleId], matrices[shapes[j].matrixId]);
//                }
//
//                material = batchedTransparentPool.GetAndQueueForRelease();
//                material.SetTexture(s_MainTexKey, textureMap.GetOrDefault(array[i].textureId));
//                DrawMesh(batchedVertexData.FillMesh(), originMatrix, material);
//
//                LightList<SVGXRenderShape>.Release(ref array[i].shapes);
//            }
//
//            texturedShapeGroups.Clear();
//        }

        private void DrawBatchedTransparents(Vector2[] points, LightList<SVGXRenderShape> renderShapes, LightList<Rect> scissors, LightList<SVGXStyle> styles, LightList<SVGXMatrix> matrices) {
            BatchedVertexData batchedVertexData = vertexDataPool.GetAndQueueForRelease();

            int count = renderShapes.Count;
            if (count == 0) return;

            SVGXRenderShape[] renderShapeArray = renderShapes.Array;
            Matrix4x4 originMatrix = OriginMatrix;

            int lastTextureId = styles[renderShapeArray[0].styleId].fillTextureId;

            for (int i = 0; i < count; i++) {
                SVGXRenderShape renderShape = renderShapeArray[i];
                int currentTextureId = styles[renderShape.styleId].fillTextureId;
                if (currentTextureId != -1) {
                    lastTextureId = currentTextureId;
                    break;
                }
            }

            Material material = batchedTransparentPool.GetAndQueueForRelease();
            FontAsset fontAsset = null;

            // todo -- support stroke textures, right now we only set texture for fill

            for (int i = 0; i < count; i++) {
                SVGXRenderShape renderShape = renderShapeArray[i];
                TextInfo textInfo = renderShape.textInfo;
                int currentTextureId = styles[renderShape.styleId].fillTextureId;

                bool fontChanged = false;

                bool textureChanged = currentTextureId != -1 && currentTextureId != lastTextureId;

                if (renderShape.shape.type == SVGXShapeType.Text) {
                    // if we have a current font and stuff to render with it, draw it
                    // set current font to this text element's font
                    fontChanged = fontAsset != textInfo.spanList[0].textStyle.fontAsset;
                }

                if (fontChanged || textureChanged) {
                    UpdateFontAtlas(material, fontAsset);
                    material.SetTexture(s_MainTexKey, textureMap.GetOrDefault(lastTextureId));
                    DrawMesh(batchedVertexData.FillMesh(), originMatrix, material);
                    batchedVertexData = vertexDataPool.GetAndQueueForRelease();
                    material = batchedTransparentPool.GetAndQueueForRelease();

                    lastTextureId = currentTextureId;

                    if (fontChanged) {
                        fontAsset = textInfo.spanList[0].textStyle.fontAsset;
                    }
                }

                int gradientLookupId = renderShape.drawCallType == DrawCallType.StandardFill
                    ? styles[renderShape.styleId].fillGradientId
                    : styles[renderShape.styleId].strokeGradientId;

                GradientData gradientData = default;
                if (gradientLookupId != -1) {
                    SVGXGradient gradient = s_GradientMap.GetOrDefault(gradientLookupId);
                    int gradientId = s_GradientRowMap.GetOrDefault(gradient);
                    gradientData = new GradientData(gradientId, gradient);
                }
                else {
                    gradientData = default;
                }

                switch (renderShape.drawCallType) {
                    // todo pass array + index to avoid struct copy cost here for style + matrix
                    case DrawCallType.StandardFill:
                        batchedVertexData.CreateFillVertices(points, renderShape, scissors[renderShape.styleId], gradientData, styles[renderShape.styleId], matrices[renderShape.matrixId]);
                        break;
                    case DrawCallType.StandardStroke:
                        batchedVertexData.CreateStrokeVertices(points, renderShape, gradientData, scissors[renderShape.styleId], styles[renderShape.styleId], matrices[renderShape.matrixId]);
                        break;
                    case DrawCallType.Shadow:
                        batchedVertexData.CreateShadowVertices(points, renderShape, gradientData, styles[renderShape.styleId], matrices[renderShape.matrixId]);
                        break;
                }
            }

            material.SetTexture(s_MainTexKey, textureMap.GetOrDefault(lastTextureId));
            UpdateFontAtlas(material, fontAsset);
            DrawMesh(batchedVertexData.FillMesh(), originMatrix, material);
        }

        private static void UpdateFontAtlas(Material renderMaterial, FontAsset fontMaterial) {
            if (fontMaterial == null) return;
            renderMaterial.SetTexture(s_GlobalFontTextureKey, fontMaterial.atlas);

            // if we group fonts by channel (4 fonts per texture) then these need to be vectors 
            // where each channel is the value for the corresponding texture
            renderMaterial.SetVector(s_GlobalFontData1Key, new Vector4(
                fontMaterial.weightNormal,
                fontMaterial.weightBold,
                fontMaterial.atlas.width,
                fontMaterial.atlas.height)
            );

            renderMaterial.SetVector(s_GlobalFontData2Key, new Vector4(
                fontMaterial.gradientScale, 0, 0, 0
//                fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A),
//                fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_B),
//                fontMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_C))
            ));
        }

        private void DrawClip(SVGXDrawWave wave) {
            if (wave.clipShapes.Count == 0) {
                return;
            }

            CreateClipFillVertices(wave, ctx.points.Array);
            //DrawMesh(wave.clipMesh, OriginMatrix, stencilClipSetPool.GetAndQueueForRelease());
        }

        private void ClearClip(SVGXDrawWave wave) {
            if (wave.clipShapes.Count == 0) {
                return;
            }

            //DrawMesh(wave.clipMesh, OriginMatrix, stencilClipClearPool.GetAndQueueForRelease());
        }

        public struct GradientData {

            public readonly int rowId;
            public readonly SVGXGradient gradient;

            public GradientData(int rowId, SVGXGradient gradient) {
                this.rowId = rowId;
                this.gradient = gradient;
            }

        }

    }

}