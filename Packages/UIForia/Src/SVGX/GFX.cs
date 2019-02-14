using System;
using System.Collections.Generic;
using UIForia;
using UIForia.Extensions;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace SVGX {

    public class GFX {

        public Camera camera;
        public ImmediateRenderContext ctx;

        internal Material debugLineMaterial;
        internal Material sdfTextMaterial;
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
        private readonly MaterialPool stencilClipFillPool;
        private readonly MaterialPool simpleFillOpaquePool;
        private readonly MaterialPool simpleStrokePool;

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

            debugLineMaterial = new Material(Shader.Find("UIForia/SimpleLineSegments"));
            sdfTextMaterial = new Material(Shader.Find("UIForia/SDFText"));

            stencilClipFillPool = new MaterialPool(Shader.Find("UIForia/SimpleFillOpaque"));
            stencilClipSetPool = new MaterialPool(Shader.Find("UIForia/StencilClipSet"));
            stencilClipClearPool = new MaterialPool(Shader.Find("UIForia/StencilClipClear"));
            simpleStrokePool = new MaterialPool(Shader.Find("UIForia/JoinedPolyline"));

            Material simpleFill = new Material(Shader.Find("UIForia/SimpleFillOpaque"));

            simpleFill.SetFloat(s_StencilRefKey, 0);

            simpleFillOpaquePool = new MaterialPool(simpleFill);

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

                wave.AddDrawCall(ctx, ctx.drawCalls[i]);
            }

            waves.Add(wave);

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
                        CreateStrokeVerticesWithJoin(strokeVertexData, ctx.points, matrix, style, shapes[i].shape);
                        break;

                    case SVGXShapeType.Circle:
                        LightList<Vector3> pnts = new LightList<Vector3>();

                        Vector2 p0 = ctx.points[shape.shape.pointRange.start + 0];
                        Vector2 p1 = ctx.points[shape.shape.pointRange.start + 1];
                        Vector2 p2 = ctx.points[shape.shape.pointRange.start + 2];
                        Vector2 p3 = ctx.points[shape.shape.pointRange.start + 3];

                        pnts.Add(p0);
                        float cx = p0.x;
                        float cy = p0.y;
                        float rx = p1.x - p0.x;
                        float ry = p3.y - p1.y;
                        pnts.Add(new Vector3(cx - rx, cy));
                        const float Kappa90 = 0.5522847493f;
                        //cx-rx, cy+ry*NVG_KAPPA90, cx-rx*NVG_KAPPA90, cy+ry, cx, cy+ry
                        SVGXBezier.CubicCurve(pnts,
                            pnts[pnts.Count - 1],
                            new Vector3(cx - rx, cy + ry * Kappa90),
                            new Vector3(cx - rx * Kappa90, cy + ry),
                            new Vector3(cx, cy + ry)
                        );
//                        SVGXBezier.CubicCurve(pnts);
//                        SVGXBezier.CubicCurve(pnts);
//                        SVGXBezier.CubicCurve(pnts);

                        break;
                    case SVGXShapeType.Ellipse:
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

        private static void GroupByTexture(SVGXDrawWave wave, LightList<SVGXRenderShape> shapes, LightList<TexturedShapeGroup> retn) {
            for (int i = 0; i < shapes.Count; i++) {
                SVGXStyle style = wave.styles[shapes[i].styleId];
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

        private void CreateClipFillVertices(SVGXDrawWave wave, Vector3[] points) {
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
                SVGXMatrix matrix = wave.matrices[0]; // todo -- make this work [shape.matrixId];
                int start = shape.pointRange.start;

                switch (shape.type) {
                    case SVGXShapeType.Unset:
                        break;
                    case SVGXShapeType.Ellipse:
                    case SVGXShapeType.Circle:
                    case SVGXShapeType.Rect:

                        vertices[vertexCnt++] = matrix.Transform(points[start++]);
                        vertices[vertexCnt++] = matrix.Transform(points[start++]);
                        vertices[vertexCnt++] = matrix.Transform(points[start++]);
                        vertices[vertexCnt++] = matrix.Transform(points[start]);

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
                origin.x -= 1f;

                return Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
            }
        }

        private void Fill(SVGXDrawWave wave, LightList<SVGXRenderShape> shapes, bool stencil, int textureId = -1) {
            FillVertexData fillVertexData = s_FillVertexDataPool.Get();

            for (int i = 0; i < shapes.Count; i++) {
                SVGXRenderShape shape = shapes[i];
                SVGXStyle style = wave.styles[shape.styleId];
                SVGXMatrix matrix = wave.matrices[shape.matrixId];
                CreateFillVertices(fillVertexData, ctx.points, matrix, style, shapes[i].shape);
            }

            Mesh mesh = fillVertexData.FillMesh();

            Matrix4x4 cameraMatrix = OriginMatrix;

            if (stencil) {
                DrawMesh(mesh, cameraMatrix, stencilClipFillPool.GetAndQueueForRelease());

//                Material cutout = stencilFillOpaqueCutoutMaterial;
//                Material paint = stencilFillOpaquePaintMaterial;
//                Material clear = stencilFillOpaqueClearMaterial;
//
//                if (transparent) {
//                    cutout = stencilFillTransparentCutoutMaterial;
//                    paint = stencilFillTransparentPaintMaterial;
//                    clear = stencilFillTransparentClearMaterial;
//                }
//
//                DrawMesh(mesh, cameraMatrix, cutout);
//                DrawMesh(mesh, cameraMatrix, paint);
//                DrawMesh(mesh, cameraMatrix, clear);
            }
            else {
                if (textureId != -1) {
                    Material material = simpleFillOpaquePool.GetAndQueueForRelease();
                    material.SetTexture(s_MainTexKey, textureMap.GetOrDefault(textureId));
                    DrawMesh(mesh, cameraMatrix, material);
                }
                else {
                    DrawMesh(mesh, cameraMatrix, simpleFillOpaquePool.GetAndQueueForRelease());
                }
            }

            fillsToRelease.Add(fillVertexData);
        }

        private void DrawSimpleOpaqueFill(SVGXDrawWave wave) {
            bool isStencil = wave.clipShapes.Count > 0;
            GroupByTexture(wave, wave.opaqueFills, texturedShapeGroups);
            TexturedShapeGroup[] array = texturedShapeGroups.Array;
            for (int i = 0; i < texturedShapeGroups.Count; i++) {
                Fill(wave, array[i].shapes, isStencil, array[i].textureId);
                LightListPool<SVGXRenderShape>.Release(ref array[i].shapes);
            }

            texturedShapeGroups.Clear();
        }

        private void DrawSimpleOpaqueStroke(SVGXDrawWave wave) {
            Stroke(wave, wave.opaqueStrokes, false, false);
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

            simpleFillOpaquePool.FlushReleaseQueue();
            stencilClipSetPool.FlushReleaseQueue();
            stencilClipClearPool.FlushReleaseQueue();
            stencilClipFillPool.FlushReleaseQueue();
            simpleStrokePool.FlushReleaseQueue();

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

            SVGXDrawWave[] waveArray = waves.Array;
            for (int i = 0; i < waves.Count; i++) {
                SVGXDrawWave wave = waveArray[i];

                DrawClip(wave);

                DrawSimpleOpaqueFill(wave);
                // DrawStencilOpaqueFill(wave);            

                DrawSimpleOpaqueStroke(wave);

                ClearClip(wave);

                s_WavePool.Release(wave);
            }

            LightListPool<SVGXDrawWave>.Release(ref waves);
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

                colors[colorCnt++] = style.strokeColor;
                colors[colorCnt++] = style.strokeColor;
                colors[colorCnt++] = style.strokeColor;
                colors[colorCnt++] = style.strokeColor;

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

        internal static void CreateStrokeVerticesWithJoin(StrokeVertexData vertexData, LightList<Vector3> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
            RangeInt range = shape.pointRange;
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
            bool isClosed = shape.isClosed;

            const int cap = 1;
            const int join = 0;

            Vector3 dir = (points[range.start + 1] - points[range.start]).normalized;
            Vector3 prev = isClosed ? points[range.end - 1] : points[range.start] - dir;
            Vector3 curr = points[range.start];
            Vector3 next = points[range.start + 1];
            Vector3 far = range.length == 2
                ? points[range.start + 1] + (points[range.start + 1] - points[range.start]).normalized
                : points[range.start + 2];

            vertices[vertexCnt++] = curr;
            vertices[vertexCnt++] = next;
            vertices[vertexCnt++] = curr;
            vertices[vertexCnt++] = next;

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
                prev = points[i - 1];
                curr = points[i];
                next = points[i + 1];
                far = points[i + 2];

                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;

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
                prev = points[currIdx - 1];
                curr = points[currIdx];
                next = points[currIdx + 1];
                far = isClosed ? points[range.start] : next + (next - curr);

                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;

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
        }

        internal static void CreateFillVertices(FillVertexData vertexData, LightList<Vector3> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
            int start = shape.pointRange.start;
            int end = shape.pointRange.end;

            vertexData.EnsureAdditionalCapacity(shape.pointRange.length);

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

            // Matrix 3x3 fillTransform -> matrix defining rotation / offset / scale for fill 
            // would need a way to index into a constant buffer probably, or pass even more vertex data
            // float rotation, vec2 scale, vec2, position -> pack into 2 floats?


            if (style.fillMode == FillMode.Color) {
                color = style.fillColor;
            }

            int gradientId = 0;
            int gradientDirection = 0;

            uint fillColorModes = (uint)style.fillMode;
            
            if (style.gradientId > 0) {
                SVGXGradient gradient = s_GradientMap.GetOrDefault(style.gradientId);
                gradientId = s_GradientRowMap.GetOrDefault(gradient);
                if (gradient is SVGXLinearGradient linearGradient) {
                    gradientDirection = (int) linearGradient.direction;
                }
            }
            
            Vector4 dimensions = shape.Dimensions;

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

                    texCoords[texCoordCnt++] = dimensions;
                    texCoords[texCoordCnt++] = dimensions;
                    texCoords[texCoordCnt++] = dimensions;
                    texCoords[texCoordCnt++] = dimensions;

                    flags[flagsCnt++] = new Vector4((int) shape.type, fillColorModes, gradientId, gradientDirection);
                    flags[flagsCnt++] = new Vector4((int) shape.type, fillColorModes, gradientId, gradientDirection);
                    flags[flagsCnt++] = new Vector4((int) shape.type, fillColorModes, gradientId, gradientDirection);
                    flags[flagsCnt++] = new Vector4((int) shape.type, fillColorModes, gradientId, gradientDirection);

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
                        vertices[vertexCnt++] = points[i];
                        flags[flagsCnt++] = new Vector4((int) shape.type, fillColorModes, gradientId, gradientDirection);
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

                    vertexData.triangleIndex = triIdx + shape.pointRange.length; // todo this might be off by 1

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}