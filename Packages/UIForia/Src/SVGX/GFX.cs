using System;
using System.Collections.Generic;
using UIForia;
using UIForia.Extensions;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace SVGX {

    public class GFX {

        public Camera camera;
        public ImmediateRenderContext ctx;

        internal Material debugLineMaterial;
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

        private readonly LightList<StrokeVertexData> strokesToRelease = new LightList<StrokeVertexData>();
        private readonly LightList<FillVertexData> fillsToRelease = new LightList<FillVertexData>();

        private readonly MaterialPool stencilClipSetPool;
        private readonly MaterialPool stencilClipClearPool;
        private readonly MaterialPool simpleStrokePool;
        private readonly MaterialPool batchedTransparentPool;
        private readonly DeferredReleasePool<BatchedVertexData> vertexDataPool;
        
        private readonly LightList<Vector2> scratchPointList;
        
        private static readonly int s_StencilRefKey = Shader.PropertyToID("_StencilRef");
        private static readonly int s_MainTexKey = Shader.PropertyToID("_MainTex");

        static GFX() {
            s_GradientRowMap = new Dictionary<SVGXGradient, int>();
            s_GradientMap = new IntMap<SVGXGradient>();
            s_WavePool = new ObjectPool<SVGXDrawWave>(null, (wave) => wave.Clear());
            s_FillVertexDataPool = new ObjectPool<FillVertexData>((a) => a.Clear());
            s_StrokeVertexDataPool = new ObjectPool<StrokeVertexData>((a) => a.Clear());
            s_SimpleFillPool = new MaterialPool(new Material(Shader.Find("UIForia/SimpleFillOpaque")));
        }

        public GFX(Camera camera) {
            this.camera = camera;

            scratchPointList = new LightList<Vector2>(64);
            debugLineMaterial = new Material(Shader.Find("UIForia/SimpleLineSegments"));
            vertexDataPool = new DeferredReleasePool<BatchedVertexData>();
            
            stencilClipSetPool = new MaterialPool(Shader.Find("UIForia/StencilClipSet"));
            stencilClipClearPool = new MaterialPool(Shader.Find("UIForia/StencilClipClear"));
            simpleStrokePool = new MaterialPool(Shader.Find("UIForia/JoinedPolyline"));

            batchedTransparentPool = new MaterialPool(Shader.Find("UIForia/BatchedTransparent"));

            Material simpleFill = new Material(Shader.Find("UIForia/SimpleFillOpaque"));

            simpleFill.SetFloat(s_StencilRefKey, 0);

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

        public void DrawDebugLine(Vector3 start, Vector3 end, Color color, float thickness = 1f) {
            StrokeVertexData strokeVertexData = s_StrokeVertexDataPool.Get();

            SVGXMatrix matrix = SVGXMatrix.identity;
            SVGXStyle style = new SVGXStyle();
            style.strokeColor = color;
            style.strokeWidth = thickness;
            SVGXShape shape = new SVGXShape(SVGXShapeType.Path, new RangeInt(0, 2));

            LightList<Vector3> points = LightListPool<Vector3>.Get();

            points.Add(start);
            points.Add(end);

            CreateStrokeVertices(strokeVertexData, points, matrix, style, shape);

            Mesh mesh = strokeVertexData.FillMesh();

            Matrix4x4 cameraMatrix = Matrix4x4.TRS(camera.transform.position + new Vector3(0, 0, 2), Quaternion.identity, Vector3.one);

            DrawMesh(mesh, cameraMatrix, debugLineMaterial);

            LightListPool<Vector3>.Release(ref points);
            strokesToRelease.Add(strokeVertexData);
        }

        public void DrawMesh(Mesh mesh, Matrix4x4 transform, Material material) {
            material.renderQueue = drawCallCnt++;
            Graphics.DrawMesh(mesh, transform, material, 0, camera, 0, null, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
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

            int lastClipId = ctx.drawCalls[0].clipGroupId;
            LightList<SVGXDrawWave> waves = LightListPool<SVGXDrawWave>.Get();

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

        private void Stroke(SVGXDrawWave wave, LightList<SVGXRenderShape> shapes, bool transparent, bool stencil) {
            if (shapes.Count == 0) return;

            StrokeVertexData strokeVertexData = s_StrokeVertexDataPool.Get();
            for (int i = 0; i < shapes.Count; i++) {
                SVGXRenderShape shape = shapes[i];
                SVGXStyle style = wave.styles[shape.styleId];
                SVGXMatrix matrix = wave.matrices[shape.matrixId];
                switch (shape.shape.type) {
                    case SVGXShapeType.Unset:
                        break;
                    case SVGXShapeType.Rect:
                        break;
                    case SVGXShapeType.Path:
                    case SVGXShapeType.RoundedRect:
                        CreateStrokeVerticesWithJoin(strokeVertexData, ctx.points, matrix, style, shapes[i]);
                        break;

                    case SVGXShapeType.Circle:
                    case SVGXShapeType.Ellipse:
                        scratchPointList.Clear();

                        Vector2 p0 = ctx.points[shape.shape.pointRange.start + 0];
                        Vector2 p2 = ctx.points[shape.shape.pointRange.start + 2];

                        float rx = (p2.x - p0.x) * 0.5f;
                        float ry = (p2.y - p0.y) * 0.5f;
                        float cx = p0.x + rx;
                        float cy = p0.y + ry;

                        scratchPointList.Add(p0 + new Vector2(0, ry));

                        const float Kappa90 = 0.5522847493f;
                        SVGXBezier.CubicCurve(scratchPointList,
                            scratchPointList[scratchPointList.Count - 1],
                            new Vector2(cx - rx, cy + ry * Kappa90),
                            new Vector2(cx - rx * Kappa90, cy + ry),
                            new Vector2(cx, cy + ry)
                        );
                        SVGXBezier.CubicCurve(
                            scratchPointList,
                            scratchPointList[scratchPointList.Count - 1],
                            new Vector2(cx + rx * Kappa90, cy + ry),
                            new Vector2(cx + rx, cy + ry * Kappa90),
                            new Vector2(cx + rx, cy)
                        );
                        SVGXBezier.CubicCurve(
                            scratchPointList,
                            scratchPointList[scratchPointList.Count - 1],
                            new Vector2(cx + rx, cy - ry * Kappa90),
                            new Vector2(cx + rx * Kappa90, cy - ry),
                            new Vector2(cx, cy - ry)
                        );
                        SVGXBezier.CubicCurve(
                            scratchPointList,
                            scratchPointList[scratchPointList.Count - 1],
                            new Vector2(cx - rx * Kappa90, cy - ry),
                            new Vector2(cx - rx, cy - ry * Kappa90),
                            new Vector2(cx - rx, cy)
                        );
                        SVGXShape proxy = new SVGXShape();
                        proxy.pointRange = new RangeInt(0, scratchPointList.Count);
                        proxy.bounds = shape.shape.bounds;
                        proxy.type = shape.shape.type;
                        proxy.isClosed = true;
                        SVGXRenderShape s = new SVGXRenderShape(proxy, shape.zIndex, shape.styleId, shape.matrixId, DrawCallType.StandardStroke);
                        CreateStrokeVerticesWithJoin(strokeVertexData, scratchPointList, matrix, style, s);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Mesh mesh = strokeVertexData.FillMesh();

            Matrix4x4 cameraMatrix = OriginMatrix;
            Material mat = simpleStrokePool.GetAndQueueForRelease();
            DrawMesh(mesh, cameraMatrix, mat);

//            if (stencil) {
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
//            }
//            else {
//                if (transparent) {
//                    DrawMesh(mesh, cameraMatrix, simpleStrokeTransparentMaterial);
//                }
//                else {
//                    DrawMesh(mesh, cameraMatrix, simpleStrokeOpaqueMaterial);
//                }
//            }

            strokesToRelease.Add(strokeVertexData);
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
                //  origin.x -= 0.5f * Screen.width;
                //  origin.y += 0.5f * Screen.height;
                origin.z += 2;
                origin.x -= 1f;

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

            stencilClipSetPool.FlushReleaseQueue();
            stencilClipClearPool.FlushReleaseQueue();
            simpleStrokePool.FlushReleaseQueue();
            vertexDataPool.FlushReleaseQueue();
            batchedTransparentPool.FlushReleaseQueue();

            textureMap.Clear();
            s_GradientMap.Clear();
            s_GradientRowMap.Clear();

            for (int i = 0; i < strokesToRelease.Count; i++) {
                s_StrokeVertexDataPool.Release(strokesToRelease[i]);
            }

            for (int i = 0; i < fillsToRelease.Count; i++) {
                s_FillVertexDataPool.Release(fillsToRelease[i]);
            }

            strokesToRelease.Clear();
            fillsToRelease.Clear();

            s_SimpleFillPool.FlushReleaseQueue();

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

            LightList<SVGXStyle> styles = new LightList<SVGXStyle>();
            LightList<SVGXMatrix> matrices = new LightList<SVGXMatrix>();

            SVGXDrawWave[] waveArray = waves.Array;
            for (int i = 0; i < waves.Count; i++) {
                SVGXDrawWave wave = waveArray[i];

                DrawClip(wave);

                LightList<SVGXRenderShape> opaques = new LightList<SVGXRenderShape>();
                LightList<SVGXRenderShape> transparents = new LightList<SVGXRenderShape>();

                for (int j = 0; j < wave.drawCalls.Count; j++) {
                    // for each shape in draw call

                    SVGXDrawCall call = wave.drawCalls[j];
                    styles.Add(call.style);
                    matrices.Add(call.matrix);

                    bool isFillTransparent = call.style.IsFillTransparent;

                    if (call.type == DrawCallType.StandardStroke) {
                        for (int k = call.shapeRange.start; k < call.shapeRange.end; k++) {
                            transparents.Add(new SVGXRenderShape(ctx.shapes[k], 0, j, j, DrawCallType.StandardStroke));
                        }
                    }
                    else if (call.type == DrawCallType.StandardFill) {
                        for (int k = call.shapeRange.start; k < call.shapeRange.end; k++) {
                            if (isFillTransparent || ctx.shapes[k].RequiresTransparentRendering) {
                                transparents.Add(new SVGXRenderShape(ctx.shapes[k], 0, j, j, DrawCallType.StandardFill));
                            }
                            else {
                                opaques.Add(new SVGXRenderShape(ctx.shapes[k], 0, j, j, DrawCallType.StandardFill));
                            }
                        }
                    }
                }

                DrawBatchedOpaques(ctx.points.Array, opaques, styles, matrices);
                DrawBatchedTransparents(ctx.points.Array, transparents, styles, matrices);

                ClearClip(wave);

                s_WavePool.Release(wave);
            }

            LightListPool<SVGXDrawWave>.Release(ref waves);
        }

        private static void GroupByTexture(LightList<SVGXStyle> styles, LightList<SVGXRenderShape> shapes, LightList<TexturedShapeGroup> retn) {
            for (int i = 0; i < shapes.Count; i++) {
                SVGXStyle style = styles[shapes[i].styleId];
                if ((style.fillMode & FillMode.Texture) != 0) {
                    int textureId = style.textureId;
                    int idx = retn.FindIndex(textureId, ((group, texId) => group.textureId == texId));
                    if (idx == -1) {
                        TexturedShapeGroup group = new TexturedShapeGroup() {
                            textureId = textureId,
                            shapes = LightListPool<SVGXRenderShape>.Get()
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
                            shapes = LightListPool<SVGXRenderShape>.Get()
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

        public struct GradientData {

            public readonly int rowId;
            public readonly SVGXGradient gradient;
            
            public GradientData(int rowId, SVGXGradient gradient) {
                this.rowId = rowId;
                this.gradient = gradient;
            }

        }

        private void DrawBatchedOpaques(Vector2[] points, LightList<SVGXRenderShape> renderShapes, LightList<SVGXStyle> styles, LightList<SVGXMatrix> matrices) {
            GroupByTexture(styles, renderShapes, texturedShapeGroups);
            TexturedShapeGroup[] array = texturedShapeGroups.Array;

            for (int i = 0; i < texturedShapeGroups.Count; i++) {
                LightList<SVGXRenderShape> shapes = array[i].shapes;
                BatchedVertexData batchedVertexData = vertexDataPool.GetAndQueueForRelease();

                for (int j = 0; j < shapes.Count; j++) {
                    SVGXGradient gradient = s_GradientMap.GetOrDefault(styles[shapes[i].styleId].gradientId);
                    int gradientId = s_GradientRowMap.GetOrDefault(gradient);
                    GradientData gradientData = new GradientData(gradientId, gradient);
                    batchedVertexData.CreateFillVertices(points, shapes[i], gradientData, styles[shapes[i].styleId], matrices[shapes[i].matrixId]);
                }

                Material material = batchedTransparentPool.GetAndQueueForRelease();
                material.SetTexture(s_MainTexKey, textureMap.GetOrDefault(array[i].textureId));
                DrawMesh(batchedVertexData.FillMesh(), OriginMatrix, material);

                LightListPool<SVGXRenderShape>.Release(ref array[i].shapes);
            }

            texturedShapeGroups.Clear();
        }

        private void DrawBatchedTransparents(Vector2[] points, LightList<SVGXRenderShape> renderShapes, LightList<SVGXStyle> styles, LightList<SVGXMatrix> matrices) {
            BatchedVertexData batchedVertexData = vertexDataPool.GetAndQueueForRelease();

            int count = renderShapes.Count;
            SVGXRenderShape[] renderShapeArray = renderShapes.Array;
            int lastTextureId = styles[renderShapeArray[0].styleId].textureId;
            Matrix4x4 originMatrix = OriginMatrix;

            GradientData gradientData = default;
            int gradientLookupId = styles[renderShapeArray[0].styleId].gradientId;
            
            if (gradientLookupId != -1) {
                SVGXGradient gradient = s_GradientMap.GetOrDefault(gradientLookupId);
                int gradientId = s_GradientRowMap.GetOrDefault(gradient);
                gradientData = new GradientData(gradientId, gradient);
            }

            if (renderShapeArray[0].drawCallType == DrawCallType.StandardFill) {
                batchedVertexData.CreateFillVertices(points, renderShapeArray[0], gradientData, styles[renderShapeArray[0].styleId], matrices[renderShapeArray[0].matrixId]);
            }
            else if (renderShapeArray[0].drawCallType == DrawCallType.StandardStroke) {
                batchedVertexData.CreateStrokeVertices(points, renderShapeArray[0], gradientData, styles[renderShapeArray[0].styleId], matrices[renderShapeArray[0].matrixId]);
            }

            Material material = null;
            for (int i = 1; i < count; i++) {
                SVGXRenderShape renderShape = renderShapeArray[i];
                int currentTextureId = styles[renderShape.styleId].textureId;

                if (currentTextureId != lastTextureId) {
                    material = batchedTransparentPool.GetAndQueueForRelease();
                    material.SetTexture(s_MainTexKey, textureMap.GetOrDefault(currentTextureId));
                    DrawMesh(batchedVertexData.FillMesh(), originMatrix, material);
                    batchedVertexData = vertexDataPool.GetAndQueueForRelease();
                }

                if (gradientLookupId != -1) {
                    SVGXGradient gradient = s_GradientMap.GetOrDefault(gradientLookupId);
                    int gradientId = s_GradientRowMap.GetOrDefault(gradient);
                    gradientData = new GradientData(gradientId, gradient);
                }
                else {
                    gradientData = default;
                }
                
                if (renderShape.drawCallType == DrawCallType.StandardFill) {
                    batchedVertexData.CreateFillVertices(points, renderShape, gradientData, styles[renderShape.styleId], matrices[renderShape.matrixId]);
                }
                else if (renderShape.drawCallType == DrawCallType.StandardStroke) {
                    batchedVertexData.CreateStrokeVertices(points, renderShape, gradientData, styles[renderShape.styleId], matrices[renderShape.matrixId]);
                }
                
            }

            material = batchedTransparentPool.GetAndQueueForRelease();
            material.SetTexture(s_MainTexKey, textureMap.GetOrDefault(lastTextureId));
            DrawMesh(batchedVertexData.FillMesh(), originMatrix, material);
        }

        private void DrawClip(SVGXDrawWave wave) {
            if (wave.clipShapes.Count == 0) {
                return;
            }

            CreateClipFillVertices(wave, ctx.points.Array);
            DrawMesh(wave.clipMesh, OriginMatrix, stencilClipSetPool.GetAndQueueForRelease());
        }

        private void ClearClip(SVGXDrawWave wave) {
            if (wave.clipShapes.Count == 0) {
                return;
            }

            DrawMesh(wave.clipMesh, OriginMatrix, stencilClipClearPool.GetAndQueueForRelease());
        }

        internal static void CreateStrokeVertices(StrokeVertexData vertexData, LightList<Vector3> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
            int triIdx = vertexData.triangleIndex;
            int vertexCnt = vertexData.position.Count;
            int colorCnt = vertexData.colors.Count;
            int texCoordCnt = vertexData.texCoords.Count;
            int flagCnt = vertexData.flags.Count;
            int prevNextCnt = vertexData.prevNext.Count;
            int triangleCnt = vertexData.triangles.Count;

            Vector3[] vertices = vertexData.position.Array;
            Vector2[] texCoords = vertexData.texCoords.Array;
            Vector4[] flags = vertexData.flags.Array;
            Vector4[] prevNext = vertexData.prevNext.Array;
            Color[] colors = vertexData.colors.Array;
            int[] triangles = vertexData.triangles.Array;

            RangeInt range = shape.pointRange;

            float thickness = Mathf.Max(0.5f, style.strokeWidth);

            Color color = style.strokeColor;
            color.a *= style.strokeOpacity;

            for (int i = range.start; i < range.end - 1; i++) {
                Vector3 pnt0 = matrix.Transform(points[i]);
                Vector3 pnt1 = matrix.Transform(points[i + 1]);

                vertices[vertexCnt++] = pnt0;
                vertices[vertexCnt++] = pnt0;
                vertices[vertexCnt++] = pnt0;
                vertices[vertexCnt++] = pnt0;

                prevNext[prevNextCnt++] = new Vector4(pnt0.x, pnt0.y, pnt1.x, pnt1.y);
                prevNext[prevNextCnt++] = new Vector4(pnt0.x, pnt0.y, pnt1.x, pnt1.y);
                prevNext[prevNextCnt++] = new Vector4(pnt0.x, pnt0.y, pnt1.x, pnt1.y);
                prevNext[prevNextCnt++] = new Vector4(pnt0.x, pnt0.y, pnt1.x, pnt1.y);

                flags[flagCnt++] = new Vector4(thickness, 0, 0, 0);
                flags[flagCnt++] = new Vector4(thickness, 1, 0, 0);
                flags[flagCnt++] = new Vector4(thickness, 2, 0, 0);
                flags[flagCnt++] = new Vector4(thickness, 3, 0, 0);

                texCoords[texCoordCnt++] = new Vector2(0, 0);
                texCoords[texCoordCnt++] = new Vector2(1, 0);
                texCoords[texCoordCnt++] = new Vector2(1, 1);
                texCoords[texCoordCnt++] = new Vector2(0, 1);

                triangles[triangleCnt++] = triIdx + 0;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 3;
                triangles[triangleCnt++] = triIdx + 0;

                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;

                triIdx += 4;
            }

            vertexData.position.Count = vertexCnt;
            vertexData.triangles.Count = triangleCnt;
            vertexData.colors.Count = colorCnt;
            vertexData.flags.Count = flagCnt;
            vertexData.texCoords.Count = texCoordCnt;
            vertexData.prevNext.Count = prevNextCnt;

            vertexData.triangleIndex = triIdx;
        }

        internal static void CreateStrokeVerticesWithJoin(StrokeVertexData vertexData, LightList<Vector2> points, SVGXMatrix matrix, SVGXStyle style, SVGXRenderShape renderShape) {
            RangeInt range = renderShape.shape.pointRange;
            vertexData.EnsureAdditionalCapacity(range.length * 4);

            int triIdx = vertexData.triangleIndex;
            int vertexCnt = vertexData.position.Count;
            int colorCnt = vertexData.colors.Count;
            int texCoordCnt = vertexData.texCoords.Count;
            int flagCnt = vertexData.flags.Count;
            int prevNextCnt = vertexData.prevNext.Count;
            int triangleCnt = vertexData.triangles.Count;

            Vector3[] vertices = vertexData.position.Array;
            Vector2[] texCoords = vertexData.texCoords.Array;
            Vector4[] flags = vertexData.flags.Array;
            Vector4[] prevNext = vertexData.prevNext.Array;
            Color[] colors = vertexData.colors.Array;
            int[] triangles = vertexData.triangles.Array;

            float strokeWidth = Mathf.Clamp(style.strokeWidth, 1, style.strokeWidth);
            Color color = style.strokeColor;
            color.a *= style.strokeOpacity;

            bool isClosed = renderShape.shape.isClosed;

            const int cap = 1;
            const int join = 0;

            float z = renderShape.zIndex;

            LightList<Vector2> transformedPoints = LightListPool<Vector2>.Get();

            transformedPoints.EnsureCapacity(range.length);
            Vector2[] transformedArray = transformedPoints.Array;

            for (int i = range.start, arrayCounter = 0; i < range.end; i++, arrayCounter++) {
                transformedArray[arrayCounter] = matrix.Transform(points[i]);
            }

            transformedPoints.Count = range.length;

            range.start = 0;

            Vector2 dir = (transformedArray[range.start + 1] - transformedArray[range.start]).normalized;
            Vector2 prev = isClosed ? transformedArray[range.end - 1] : transformedArray[range.start] - dir;
            Vector2 curr = transformedArray[range.start];
            Vector2 next = transformedArray[range.start + 1];
            Vector2 far = range.length == 2
                ? transformedArray[range.start + 1] + (transformedArray[range.start + 1] - transformedArray[range.start]).normalized
                : transformedArray[range.start + 2];

            vertices[vertexCnt++] = new Vector3(curr.x, curr.y, z);
            vertices[vertexCnt++] = new Vector3(next.x, next.y, z);
            vertices[vertexCnt++] = new Vector3(curr.x, curr.y, z);
            vertices[vertexCnt++] = new Vector3(next.x, next.y, z);

            flags[flagCnt++] = new Vector4(1, 0, cap, strokeWidth);
            flags[flagCnt++] = new Vector4(1, 1, range.length > 2 ? join : cap, strokeWidth);
            flags[flagCnt++] = new Vector4(-1, 2, cap, strokeWidth);
            flags[flagCnt++] = new Vector4(-1, 3, range.length > 2 ? join : cap, strokeWidth);

            prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
            prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
            prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
            prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

            texCoords[texCoordCnt++] = new Vector2(0, 1);
            texCoords[texCoordCnt++] = new Vector2(1, 1);
            texCoords[texCoordCnt++] = new Vector2(1, 0);
            texCoords[texCoordCnt++] = new Vector2(0, 0);

            colors[colorCnt++] = color;
            colors[colorCnt++] = color;
            colors[colorCnt++] = color;
            colors[colorCnt++] = color;

            triangles[triangleCnt++] = triIdx + 0;
            triangles[triangleCnt++] = triIdx + 1;
            triangles[triangleCnt++] = triIdx + 2;
            triangles[triangleCnt++] = triIdx + 2;
            triangles[triangleCnt++] = triIdx + 1;
            triangles[triangleCnt++] = triIdx + 3;

            triIdx += 4;

            for (int i = range.start + 1; i < range.end - 2; i++) {
                prev = transformedArray[i - 1];
                curr = transformedArray[i];
                next = transformedArray[i + 1];
                far = transformedArray[i + 2];

                vertices[vertexCnt++] = new Vector3(curr.x, curr.y, z);
                vertices[vertexCnt++] = new Vector3(next.x, next.y, z);
                vertices[vertexCnt++] = new Vector3(curr.x, curr.y, z);
                vertices[vertexCnt++] = new Vector3(next.x, next.y, z);

                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                flags[flagCnt++] = new Vector4(1, 0, join, strokeWidth);
                flags[flagCnt++] = new Vector4(1, 1, join, strokeWidth);
                flags[flagCnt++] = new Vector4(-1, 2, join, strokeWidth);
                flags[flagCnt++] = new Vector4(-1, 3, join, strokeWidth);

                texCoords[texCoordCnt++] = new Vector2(0, 0);
                texCoords[texCoordCnt++] = new Vector2(1, 0);
                texCoords[texCoordCnt++] = new Vector2(1, 1);
                texCoords[texCoordCnt++] = new Vector2(0, 1);

                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;

                triangles[triangleCnt++] = triIdx + 0;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 3;

                triIdx += 4;
            }

            if (range.length > 2) {
                int currIdx = range.end - 2;
                prev = transformedArray[currIdx - 1];
                curr = transformedArray[currIdx];
                next = transformedArray[currIdx + 1];
                far = isClosed ? transformedArray[range.start + 1] : next + (next - curr);

                vertices[vertexCnt++] = new Vector3(curr.x, curr.y, z);
                vertices[vertexCnt++] = new Vector3(next.x, next.y, z);
                vertices[vertexCnt++] = new Vector3(curr.x, curr.y, z);
                vertices[vertexCnt++] = new Vector3(next.x, next.y, z);

                flags[flagCnt++] = new Vector4(1, 0, join, strokeWidth);
                flags[flagCnt++] = new Vector4(1, 1, join, strokeWidth);
                flags[flagCnt++] = new Vector4(-1, 2, join, strokeWidth);
                flags[flagCnt++] = new Vector4(-1, 3, join, strokeWidth);

                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                texCoords[texCoordCnt++] = new Vector2(0, 1);
                texCoords[texCoordCnt++] = new Vector2(1, 1);
                texCoords[texCoordCnt++] = new Vector2(1, 0);
                texCoords[texCoordCnt++] = new Vector2(0, 0);

                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;

                triangles[triangleCnt++] = triIdx + 0;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 3;

                triIdx += 4;

                currIdx++;

                prev = transformedArray[currIdx - 1];
                curr = transformedArray[currIdx];
                next = isClosed ? transformedArray[range.start + 1] : curr + (curr - prev).normalized;
                far = isClosed ? transformedArray[range.start + 2] : next + (next - curr).normalized;

                vertices[vertexCnt++] = new Vector3(curr.x, curr.y, z);
                vertices[vertexCnt++] = new Vector3(next.x, next.y, z);
                vertices[vertexCnt++] = new Vector3(curr.x, curr.y, z);
                vertices[vertexCnt++] = new Vector3(next.x, next.y, z);

                flags[flagCnt++] = new Vector4(1, 0, join, strokeWidth);
                flags[flagCnt++] = new Vector4(1, 1, isClosed ? join : cap, strokeWidth);
                flags[flagCnt++] = new Vector4(-1, 2, join, strokeWidth);
                flags[flagCnt++] = new Vector4(-1, 3, isClosed ? join : cap, strokeWidth);

                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                texCoords[texCoordCnt++] = new Vector2(0, 1);
                texCoords[texCoordCnt++] = new Vector2(1, 1);
                texCoords[texCoordCnt++] = new Vector2(1, 0);
                texCoords[texCoordCnt++] = new Vector2(0, 0);

                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;
                colors[colorCnt++] = color;

                triangles[triangleCnt++] = triIdx + 0;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 3;

                triIdx += 4;
            }

            vertexData.position.Count = vertexCnt;
            vertexData.triangles.Count = triangleCnt;
            vertexData.colors.Count = colorCnt;
            vertexData.flags.Count = flagCnt;
            vertexData.texCoords.Count = texCoordCnt;
            vertexData.prevNext.Count = prevNextCnt;

            vertexData.triangleIndex = triIdx;
            LightListPool<Vector2>.Release(ref transformedPoints);
        }

        internal static void CreateFillVertices(FillVertexData vertexData, LightList<Vector2> points, SVGXMatrix matrix, SVGXStyle style, SVGXRenderShape renderShape) {
            int start = renderShape.shape.pointRange.start;
            int end = renderShape.shape.pointRange.end;

            vertexData.EnsureAdditionalCapacity(renderShape.shape.pointRange.length);

            int triIdx = vertexData.triangleIndex;
            int vertexCnt = vertexData.position.Count;
            int colorCnt = vertexData.colors.Count;
            int texCoordCnt = vertexData.texCoords.Count;
            int flagsCnt = vertexData.flags.Count;
            int triangleCnt = vertexData.triangles.Count;

            Vector3[] vertices = vertexData.position.Array;
            Vector4[] texCoords = vertexData.texCoords.Array;
            Vector4[] flags = vertexData.flags.Array;
            Color[] colors = vertexData.colors.Array;
            int[] triangles = vertexData.triangles.Array;

            Color color = style.fillColor;

            if (style.fillMode == FillMode.Color) {
                color = style.fillColor;
            }
            else if ((style.fillMode & FillMode.Tint) != 0) {
                color = style.fillTintColor;
            }

            int gradientId = 0;
            int gradientDirection = 0;

            uint fillColorModes = (uint) style.fillMode;

            if (style.gradientId > 0) {
                SVGXGradient gradient = s_GradientMap.GetOrDefault(style.gradientId);
                gradientId = s_GradientRowMap.GetOrDefault(gradient);
                if (gradient is SVGXLinearGradient linearGradient) {
                    gradientDirection = (int) linearGradient.direction;
                }
            }

            Vector4 dimensions = renderShape.shape.Dimensions;

            float z = renderShape.zIndex;
            float opacity = style.fillOpacity;
            color.a *= opacity;

            LightList<Vector2> transformedPoints = LightListPool<Vector2>.Get();
            transformedPoints.EnsureCapacity(renderShape.shape.pointRange.length);

            Vector2[] transformedArray = transformedPoints.Array;

            for (int i = start, idx = 0; i < end; i++, idx++) {
                transformedArray[idx] = matrix.Transform(points[i]);
            }

            start = 0;
            end = renderShape.shape.pointRange.length;

            switch (renderShape.shape.type) {
                case SVGXShapeType.Unset:
                    break;
                case SVGXShapeType.Ellipse:
                case SVGXShapeType.Circle:
                case SVGXShapeType.Rect:

                    Vector2 p0 = transformedArray[start++];
                    Vector2 p1 = transformedArray[start++];
                    Vector2 p2 = transformedArray[start++];
                    Vector2 p3 = transformedArray[start];

                    vertices[vertexCnt++] = new Vector3(p0.x, p0.y, z);
                    vertices[vertexCnt++] = new Vector3(p1.x, p1.y, z);
                    vertices[vertexCnt++] = new Vector3(p2.x, p2.y, z);
                    vertices[vertexCnt++] = new Vector3(p3.x, p3.y, z);

                    triangles[triangleCnt++] = triIdx + 0;
                    triangles[triangleCnt++] = triIdx + 1;
                    triangles[triangleCnt++] = triIdx + 2;
                    triangles[triangleCnt++] = triIdx + 2;
                    triangles[triangleCnt++] = triIdx + 3;
                    triangles[triangleCnt++] = triIdx + 0;

                    texCoords[texCoordCnt++] = new Vector4(0, 1);
                    texCoords[texCoordCnt++] = new Vector4(1, 1);
                    texCoords[texCoordCnt++] = new Vector4(1, 0);
                    texCoords[texCoordCnt++] = new Vector4(0, 0);

                    flags[flagsCnt++] = new Vector4((int) renderShape.shape.type, fillColorModes, gradientId, gradientDirection);
                    flags[flagsCnt++] = new Vector4((int) renderShape.shape.type, fillColorModes, gradientId, gradientDirection);
                    flags[flagsCnt++] = new Vector4((int) renderShape.shape.type, fillColorModes, gradientId, gradientDirection);
                    flags[flagsCnt++] = new Vector4((int) renderShape.shape.type, fillColorModes, gradientId, gradientDirection);

                    colors[colorCnt++] = color;
                    colors[colorCnt++] = color;
                    colors[colorCnt++] = color;
                    colors[colorCnt++] = color;

                    vertexData.position.Count = vertexCnt;
                    vertexData.triangles.Count = triangleCnt;
                    vertexData.colors.Count = colorCnt;
                    vertexData.texCoords.Count = texCoordCnt;
                    vertexData.flags.Count = flagsCnt;

                    vertexData.triangleIndex = triIdx + 4;

                    break;

                case SVGXShapeType.Path:

                    // assume closed for now
                    // assume convex for now

                    throw new NotImplementedException();

                case SVGXShapeType.RoundedRect: // or other convex shape without holes

                    for (int i = start; i < end; i++) {
                        vertices[vertexCnt++] = new Vector3(transformedArray[i].x, transformedArray[i].y, z);
                        flags[flagsCnt++] = new Vector4((int) renderShape.shape.type, fillColorModes, gradientId, gradientDirection);
                        colors[colorCnt++] = color;
                        texCoords[texCoordCnt++] = dimensions;
                    }

                    int t = 0;
                    for (int i = start; i < end - 1; i++) {
                        triangles[triangleCnt++] = triIdx + 0;
                        triangles[triangleCnt++] = triIdx + t;
                        triangles[triangleCnt++] = triIdx + t + 1;
                        t++;
                    }

                    vertexData.position.Count = vertexCnt;
                    vertexData.triangles.Count = triangleCnt;
                    vertexData.colors.Count = colorCnt;
                    vertexData.texCoords.Count = texCoordCnt;
                    vertexData.flags.Count = flagsCnt;

                    vertexData.triangleIndex = triIdx + renderShape.shape.pointRange.length; // todo this might be off by 1

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            LightListPool<Vector2>.Release(ref transformedPoints);
        }

    }

}