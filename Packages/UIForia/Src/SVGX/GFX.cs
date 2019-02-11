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

        internal Material stencilFillOpaqueCutoutMaterial;
        internal Material stencilFillOpaquePaintMaterial;
        internal Material stencilFillOpaqueClearMaterial;

        internal Material stencilFillTransparentCutoutMaterial;
        internal Material stencilFillTransparentPaintMaterial;
        internal Material stencilFillTransparentClearMaterial;

        internal Material stencilStrokeOpaqueCutoutMaterial;
        internal Material stencilStrokeOpaquePaintMaterial;
        internal Material stencilStrokeOpaqueClearMaterial;

        internal Material stencilStrokeTransparentCutoutMaterial;
        internal Material stencilStrokeTransparentPaintMaterial;
        internal Material stencilStrokeTransparentClearMaterial;

        internal Material simpleFillOpaqueMaterial;
        internal Material simpleFillTransparentMaterial;

        internal Material simpleStrokeOpaqueMaterial;
        internal Material simpleStrokeTransparentMaterial;

        internal Material debugLineMaterial;
        internal Material sdfTextMaterial;
        
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

        public void DrawDebugLine(Vector2 start, Vector2 end, Color color, float thickness = 1f) {
            StrokeVertexData strokeVertexData = s_StrokeVertexDataPool.Get();

            SVGXMatrix matrix = SVGXMatrix.identity;
            SVGXStyle style = new SVGXStyle();
            style.strokeColor = color;
            style.strokeWidth = thickness;
            SVGXShape shape = new SVGXShape(SVGXShapeType.Path, new RangeInt(0, 2));
            
            LightList<Vector2> points = LightListPool<Vector2>.Get();
            
            points.Add(start);
            points.Add(end);
            
            CreateStrokeVertices(strokeVertexData, points, matrix, style, shape);

            Mesh mesh = strokeVertexData.FillMesh();
            
            Matrix4x4 cameraMatrix = Matrix4x4.TRS(camera.transform.position + new Vector3(0, 0, 2), Quaternion.identity, Vector3.one);

            DrawMesh(mesh, cameraMatrix, debugLineMaterial);

            LightListPool<Vector2>.Release(ref points);
            strokesToRelease.Add(strokeVertexData);
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

            SVGXDrawWave wave = s_WavePool.Get();

            for (int i = 0; i < ctx.drawCalls.Count; i++) {
                wave.AddDrawCall(ctx, ctx.drawCalls[i]);
            }

            return wave;
        }

        private void Stroke(SVGXDrawWave wave, LightList<SVGXRenderShape> shapes, bool transparent, bool stencil) {
            StrokeVertexData strokeVertexData = s_StrokeVertexDataPool.Get();

            for (int i = 0; i < shapes.Count; i++) {
                SVGXRenderShape shape = shapes[i];
                SVGXStyle style = wave.styles[shape.styleId];
                SVGXMatrix matrix = wave.matrices[shape.matrixId];
                CreateStrokeVerticesWithJoin(strokeVertexData, ctx.points, matrix, style, shapes[i].shape);
            }

            Mesh mesh = strokeVertexData.FillMesh();

            Matrix4x4 cameraMatrix = Matrix4x4.TRS(camera.transform.position + new Vector3(0, 0, 2), Quaternion.identity, Vector3.one);

            if (stencil) {
                Material cutout = stencilStrokeOpaqueCutoutMaterial;
                Material paint = stencilStrokeOpaquePaintMaterial;
                Material clear = stencilStrokeOpaqueClearMaterial;

                if (transparent) {
                    cutout = stencilStrokeTransparentCutoutMaterial;
                    paint = stencilStrokeTransparentPaintMaterial;
                    clear = stencilStrokeTransparentClearMaterial;
                }

                DrawMesh(mesh, cameraMatrix, cutout);
                DrawMesh(mesh, cameraMatrix, paint);
                DrawMesh(mesh, cameraMatrix, clear);
            }
            else {
                if (transparent) {
                    DrawMesh(mesh, cameraMatrix, simpleStrokeTransparentMaterial);
                }
                else {
                    DrawMesh(mesh, cameraMatrix, simpleStrokeOpaqueMaterial);
                }
            }

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
            }
        }

        private void Fill(SVGXDrawWave wave, LightList<SVGXRenderShape> shapes, bool transparent, bool stencil, int textureId = -1) {
            // todo preserve z order

            FillVertexData fillVertexData = s_FillVertexDataPool.Get();

            for (int i = 0; i < shapes.Count; i++) {
                SVGXRenderShape shape = shapes[i];
                SVGXStyle style = wave.styles[shape.styleId];
                SVGXMatrix matrix = wave.matrices[shape.matrixId];
                CreateFillVertices(fillVertexData, ctx.points, matrix, style, shapes[i].shape);
            }

            Mesh mesh = fillVertexData.FillMesh();
            Vector3 origin = camera.transform.position;
            origin.x -= 0.5f * Screen.width;
            origin.y += 0.5f * Screen.height;
            origin.z += 2;
            Matrix4x4 cameraMatrix = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);

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
                if (textureId != -1) {
                    Material material = s_SimpleFillPool.Get();
                    material.mainTexture = textureMap.GetOrDefault(textureId);
                    DrawMesh(mesh, cameraMatrix, material);
                    s_SimpleFillPool.QueueForRelease(material);
                }
                else {
                    DrawMesh(mesh, cameraMatrix, simpleFillOpaqueMaterial);
                }
            }

            s_FillVertexDataPool.Release(fillVertexData);
        }

        internal void StencilFill(SVGXDrawWave wave, LightList<SVGXRenderShape> shapes, bool transparent) {
            Fill(wave, shapes, transparent, true);
        }

        internal void SimpleFill(SVGXDrawWave wave, LightList<SVGXRenderShape> shapes, bool transparent) {
            Fill(wave, shapes, transparent, false);
        }

        private void DrawSimpleOpaqueFill(SVGXDrawWave wave) {
            GroupByTexture(wave, wave.opaqueFills, texturedShapeGroups);
            TexturedShapeGroup[] array = texturedShapeGroups.Array;
            for (int i = 0; i < texturedShapeGroups.Count; i++) {
                Fill(wave, array[i].shapes, false, false, array[i].textureId);
                LightListPool<SVGXRenderShape>.Release(ref array[i].shapes);
            }

            texturedShapeGroups.Clear();
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
            textureMap.Clear();
            s_GradientMap.Clear();
            s_GradientRowMap.Clear();
            for (int i = 0; i < strokesToRelease.Count; i++) {
                s_StrokeVertexDataPool.Release(strokesToRelease[i]);
            }
            strokesToRelease.Clear();
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

            SVGXDrawWave wave = CreateWaves(ctx);

            DrawSimpleOpaqueFill(wave);
            // DrawStencilOpaqueFill(wave);            

            DrawSimpleOpaqueStroke(wave);
            s_WavePool.Release(wave);
        }

        private static float Pack(Vector2 input, int precision) {
            Vector2 output = input;
            output.x = Mathf.Floor(output.x * (precision - 1));
            output.y = Mathf.Floor(output.y * (precision - 1));

            return (output.x * precision) + output.y;
        }

        internal void CreateStrokeVertices(StrokeVertexData vertexData, LightList<Vector2> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
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
                Vector2 pnt0 = matrix.Transform(points[i]);
                Vector2 pnt1 = matrix.Transform(points[i + 1]);

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

        internal void CreateStrokeVerticesWithJoin(StrokeVertexData vertexData, LightList<Vector2> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
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

            float strokeWidth = Mathf.Max(5, style.strokeWidth);

            style.strokeColor = Color.red;

            bool isClosed = false;

            Vector2 dir = (points[range.start + 1] - points[range.start]).normalized;
            Vector2 prev = isClosed ? points[range.end - 1] : points[range.start] - dir;
            Vector2 curr = points[range.start];
            Vector2 next = points[range.start + 1];
            Vector2 far = points.Count == 2
                ? points[range.start + 1] + (points[range.start + 1] - points[range.start]).normalized
                : points[range.start + 2];

            vertices[vertexCnt++] = curr;
            vertices[vertexCnt++] = next;
            vertices[vertexCnt++] = curr;
            vertices[vertexCnt++] = next;
            const int cap = 1;
            const int join = 2;

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

            colors[colorCnt++] = Color.blue; //color;
            colors[colorCnt++] = Color.blue; //color;
            colors[colorCnt++] = Color.blue; //color;
            colors[colorCnt++] = Color.blue; //color;

            triangles[triangleCnt++] = triIdx + 0;
            triangles[triangleCnt++] = triIdx + 1;
            triangles[triangleCnt++] = triIdx + 2;
            triangles[triangleCnt++] = triIdx + 2;
            triangles[triangleCnt++] = triIdx + 1;
            triangles[triangleCnt++] = triIdx + 3;

            triIdx += 4;

            for (int i = range.start + 1; i < range.end - 2; i++) {
                // todo -- pre-transform points
                prev = matrix.Transform(points[i - 1]);
                curr = matrix.Transform(points[i]);
                next = matrix.Transform(points[i + 1]);
                far = matrix.Transform(points[i + 2]);

                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
                vertices[vertexCnt++] = curr;
                vertices[vertexCnt++] = next;
//                
//                vertices[vertexCnt++] = curr;
//                vertices[vertexCnt++] = next;
//                vertices[vertexCnt++] = curr;
//                vertices[vertexCnt++] = next;

                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

//                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
//                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
//                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
//                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                flags[flagCnt++] = new Vector4(1, 0, join, strokeWidth);
                flags[flagCnt++] = new Vector4(1, 1, join, strokeWidth);
                flags[flagCnt++] = new Vector4(-1, 2, join, strokeWidth);
                flags[flagCnt++] = new Vector4(-1, 3, join, strokeWidth);

//                flags[flagCnt++] = new Vector4(1, 4, join, strokeWidth);
//                flags[flagCnt++] = new Vector4(1, 5, join, strokeWidth);
//                flags[flagCnt++] = new Vector4(-1, 6, join, strokeWidth);
//                flags[flagCnt++] = new Vector4(-1, 7, join, strokeWidth);

                texCoords[texCoordCnt++] = new Vector2(0, 0);
                texCoords[texCoordCnt++] = new Vector2(1, 0);
                texCoords[texCoordCnt++] = new Vector2(1, 1);
                texCoords[texCoordCnt++] = new Vector2(0, 1);

//                texCoords[texCoordCnt++] = new Vector2(0, 1);
//                texCoords[texCoordCnt++] = new Vector2(0, 1);
//                texCoords[texCoordCnt++] = new Vector2(0, 1);
//                texCoords[texCoordCnt++] = new Vector2(0, 1);

                colors[colorCnt++] = new Color(0, 0, 0, 0); //style.strokeColor;
                colors[colorCnt++] = new Color(0, 0, 0, 0); //style.strokeColor;
                colors[colorCnt++] = new Color(0, 0, 0, 0); //style.strokeColor;
                colors[colorCnt++] = new Color(0, 0, 0, 0); //style.strokeColor;

//                colors[colorCnt++] = style.strokeColor;
//                colors[colorCnt++] = style.strokeColor;
//                colors[colorCnt++] = style.strokeColor;
//                colors[colorCnt++] = style.strokeColor;

                triangles[triangleCnt++] = triIdx + 0;
                triangles[triangleCnt++] = triIdx + 1;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 2;
                triangles[triangleCnt++] = triIdx + 3;
                triangles[triangleCnt++] = triIdx + 0;

                triIdx += 4;

//                triangles[triangleCnt++] = triIdx + 0;
//                triangles[triangleCnt++] = triIdx + 1;
//                triangles[triangleCnt++] = triIdx + 2;
//                triangles[triangleCnt++] = triIdx + 2;
//                triangles[triangleCnt++] = triIdx + 3;
//                triangles[triangleCnt++] = triIdx + 0;
//                
//                triIdx += 4;
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
                flags[flagCnt++] = new Vector4(1, 1, cap, strokeWidth);
                flags[flagCnt++] = new Vector4(-1, 2, join, strokeWidth);
                flags[flagCnt++] = new Vector4(-1, 3, cap, strokeWidth);

                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);
                prevNext[prevNextCnt++] = new Vector4(prev.x, prev.y, next.x, next.y);
                prevNext[prevNextCnt++] = new Vector4(curr.x, curr.y, far.x, far.y);

                texCoords[texCoordCnt++] = new Vector2(0, 1);
                texCoords[texCoordCnt++] = new Vector2(1, 1);
                texCoords[texCoordCnt++] = new Vector2(1, 0);
                texCoords[texCoordCnt++] = new Vector2(0, 0);

                colors[colorCnt++] = new Color(0, 0, 0, 0); //style.strokeColor;
                colors[colorCnt++] = new Color(0, 0, 0, 0); //style.strokeColor;
                colors[colorCnt++] = new Color(0, 0, 0, 0); //style.strokeColor;
                colors[colorCnt++] = new Color(0, 0, 0, 0); //style.strokeColor;

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

        internal void CreateFillVertices(FillVertexData vertexData, LightList<Vector2> points, SVGXMatrix matrix, SVGXStyle style, SVGXShape shape) {
            int triIdx = vertexData.triangleIndex;
            int vertexCnt = vertexData.position.Count;
            int colorCnt = vertexData.colors.Count;
            int texCoordCnt = vertexData.texCoords.Count;
            int flagsCnt = vertexData.flags.Count;
            int triangleCnt = vertexData.triangles.Count;

            Vector3[] vertices = vertexData.position.Array;
            Vector2[] texCoords = vertexData.texCoords.Array;
            Vector4[] flags = vertexData.flags.Array;
            Color[] colors = vertexData.colors.Array;
            int[] triangles = vertexData.triangles.Array;

            int start = shape.pointRange.start;
            Color color = style.fillColor;

            // Matrix 3x3 fillTransform -> matrix defining rotation / offset / scale for fill 
            // would need a way to index into a constant buffer probably, or pass even more vertex data
            // float rotation, vec2 scale, vec2, position -> pack into 2 floats?

            // probably want to pre-process the points and apply the matrix transform to them in a job
            // that way we can do each point only once even if they get re-used

            switch (shape.type) {
                case SVGXShapeType.Unset:
                    break;
                case SVGXShapeType.Ellipse:
                case SVGXShapeType.Circle:
                case SVGXShapeType.Rect:

                    int fillMode = 1; //(style.fillMode & FillMode.Texture) > 0 ? 1 : 0;
                    int colorMode = 1; //(style.fillMode & FillMode.Gradient) > 0 ? 1 : 0;

                    if (style.fillMode == FillMode.Color) {
                        color = style.fillColor;
                    }

                    int gradientId = 0;
                    int gradientDirection = 0;
                    float fillColorModes = Pack(new Vector2(fillMode, colorMode), 4096);
                    if (style.gradientId > 0) {
                        SVGXGradient gradient = s_GradientMap.GetOrDefault(style.gradientId);
                        gradientId = s_GradientRowMap.GetOrDefault(gradient);
                        if (gradient is SVGXLinearGradient linearGradient) {
                            gradientDirection = (int) linearGradient.direction;
                        }
                    }

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
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}