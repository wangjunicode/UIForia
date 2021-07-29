using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AOT;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace UIForia.Prototype {

    public unsafe class FontLoaderPrototype : MonoBehaviour {

        public Material lineMaterial;
        public Material fillMaterial;
        public Material blendMaterial;

        public Vector2 textureSize;
        private Mesh lineMesh;
        private Mesh fillMesh;
        private Mesh fullScreenQuad;

        public Camera textCamera;
        public Vector3 translation;

        private int lastCodepoints;

        [Range(1, 850)] public int renderedCodepoints = 500;
        [Range(0, 850)] public int renderStart = 100;

        private IntPtr ctx;
        private ushort fontId;
        private int totalCodepoints;
        private uint[] codepointList;
        private int lastRenderStart;

        [BurstCompile(DisableSafetyChecks = true)]
        private struct BuildMeshJob : IJob {

            public TempList<FontLineVertex> lineList;
            public TempList<FontFillVertex> fillList;
            public NativeArray<float3> lineVertices;
            public NativeArray<int> lineIndices;
            public NativeArray<float3> fillVertices;
            public NativeArray<float4> lineUv0;
            public NativeArray<float2> lineUv1;
            public NativeArray<float2> fillUv0;
            public int renderCount;

            [NativeDisableUnsafePtrRestriction] public GlyphResponse* responses;
            public NativeArray<int> fillIndices;

            public void Execute() {
                FontAtlasPacker packer = new FontAtlasPacker(1024, 2048);

                DataList<AtlasGlyph> glyphs = new DataList<AtlasGlyph>(renderCount, Allocator.Temp);

                glyphs.size = renderCount;

                NativeSortExtension.Sort(responses, renderCount);

                for (int rId = 0; rId < renderCount; rId++) {
                    packer.TryPack(responses[rId], out glyphs[rId]);
                }

                for (int rId = 0; rId < renderCount; rId++) {

                    AtlasGlyph atlasArea = glyphs[rId];

                    Vector3 position = new Vector3(atlasArea.x, atlasArea.y, 0);
                    ref GlyphResponse response = ref responses[rId];

                    int lineStart = (int) response.lineVertexStart;
                    int lineEnd = (int) response.lineVertexCount + lineStart;
                    int fillStart = (int) response.fillVertexStart;
                    int fillEnd = (int) response.fillVertexCount + fillStart;

                    for (int i = lineStart; i < lineEnd; i++) {
                        ref FontLineVertex line = ref lineList.array[i];
                        lineVertices[i] = new float3(line.pos.x + position.x, line.pos.y + position.y, 0);
                        lineUv0[i] = new float4(line.par.x, line.par.y, line.limits.x, line.limits.y);
                        lineUv1[i] = new float2(line.scale, line.lineWidth);
                    }

                    for (int i = fillStart; i < fillEnd; i++) {
                        ref FontFillVertex fill = ref fillList.array[i];
                        fillVertices[i] = new float3(fill.pos.x + position.x, fill.pos.y + position.y, 0);
                        fillUv0[i] = fill.par;
                    }

                }

                for (int i = 0; i < fillList.size; i++) {
                    fillIndices[i] = i;
                }

                int triangleIdx = 0;
                for (int i = 0; i < lineList.size; i += 4) {
                    lineIndices[triangleIdx++] = i + 0;
                    lineIndices[triangleIdx++] = i + 1;
                    lineIndices[triangleIdx++] = i + 2;
                    lineIndices[triangleIdx++] = i + 2;
                    lineIndices[triangleIdx++] = i + 3;
                    lineIndices[triangleIdx++] = i + 0;
                }

                packer.Dispose();
                glyphs.Dispose();
            }

        }

        private void UpdateRender(int renderEnd) {

            Stopwatch total = Stopwatch.StartNew();
            GlyphRequest* requests = TypedUnsafe.Malloc<GlyphRequest>(totalCodepoints, Allocator.Temp);
            GlyphResponse* responses = TypedUnsafe.Malloc<GlyphResponse>(renderEnd, Allocator.Temp);

            int idx = 0;
            for (int i = renderStart; i < renderEnd; i++) {
                requests[idx++] = new GlyphRequest() {
                    codepoint = codepointList[i],
                    fontId = fontId,
                    pixelSize = 32,
                    sdfSize = 8
                };
            }

            int lineVertexCount = 0;
            int fillVertexCount = 0;
            Stopwatch sw = Stopwatch.StartNew();

            int renderCount = renderEnd - renderStart;

            TextServer.GenerateRenderStructures(ctx, requests, renderCount, responses, &lineVertexCount, &fillVertexCount);
            // Debug.Log("Generated in: " + sw.Elapsed.TotalMilliseconds + " ms");

            TempList<FontLineVertex> lineList = TypedUnsafe.MallocSizedTempList<FontLineVertex>(lineVertexCount, Allocator.Temp);
            TempList<FontFillVertex> fillList = TypedUnsafe.MallocSizedTempList<FontFillVertex>(fillVertexCount, Allocator.Temp);
            sw.Restart();
            TextServer.CopyGlyphBuffers(ctx, lineList.array, fillList.array);
            // Debug.Log("Extracted buffers in: " + sw.Elapsed.TotalMilliseconds + " ms");

            lineMesh = lineMesh != null ? lineMesh : new Mesh();
            fillMesh = fillMesh != null ? fillMesh : new Mesh();
            fullScreenQuad = fullScreenQuad != null ? fullScreenQuad : new Mesh();

            lineMesh.Clear(true);
            fillMesh.Clear(true);
            fullScreenQuad.Clear(true);

            sw.Restart();

            lineMesh.indexFormat = IndexFormat.UInt32;
            fillMesh.indexFormat = IndexFormat.UInt32;

            int lineQuadCount = lineList.size / 4;

            NativeArray<float3> lineVertices = new NativeArray<float3>(lineVertexCount, Allocator.TempJob);
            NativeArray<float4> lineUV0 = new NativeArray<float4>(lineVertexCount, Allocator.TempJob);
            NativeArray<float2> lineUV1 = new NativeArray<float2>(lineVertexCount, Allocator.TempJob);
            NativeArray<int> lineIndices = new NativeArray<int>(lineQuadCount * 6, Allocator.TempJob);

            NativeArray<float3> fillVertices = new NativeArray<float3>(fillList.size, Allocator.TempJob);
            NativeArray<float2> fillUvs = new NativeArray<float2>(fillList.size, Allocator.TempJob);
            NativeArray<int> fillIndices = new NativeArray<int>(fillList.size, Allocator.TempJob);

            sw.Restart();
            new BuildMeshJob() {
                responses = responses,
                lineIndices = lineIndices,
                lineList = lineList,
                lineUv0 = lineUV0,
                lineUv1 = lineUV1,
                lineVertices = lineVertices,
                fillList = fillList,
                fillVertices = fillVertices,
                fillUv0 = fillUvs,
                fillIndices = fillIndices,
                renderCount = renderCount,
            }.Run();
            
            // Debug.Log("Build meshes: " + sw.Elapsed.TotalMilliseconds + " ms");
            sw.Restart();

            lineMesh.SetVertices(lineVertices, 0, lineVertices.Length, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds);
            lineMesh.SetUVs(0, lineUV0);
            lineMesh.SetUVs(1, lineUV1);
            lineMesh.SetIndices(lineIndices, MeshTopology.Triangles, 0, false);

            fillMesh.SetVertices(fillVertices, 0, fillVertices.Length, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds);
            fillMesh.SetUVs(0, fillUvs);
            fillMesh.SetIndices(fillIndices, MeshTopology.Triangles, 0, false);

            lineVertices.Dispose();
            lineUV0.Dispose();
            lineUV1.Dispose();
            lineIndices.Dispose();
            fillVertices.Dispose();
            fillUvs.Dispose();
            fillIndices.Dispose();

            fullScreenQuad.vertices = new[] {
                new Vector3(0, 0, 1),
                new Vector3(Screen.width, 0, 1),
                new Vector3(Screen.width, Screen.height, 1),
                new Vector3(0, Screen.height, 1),
            };

            fullScreenQuad.uv = new[] {
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
            };

            fullScreenQuad.triangles = new[] {
                0, 1, 2,
                2, 3, 0
            };

            sw.Stop();

            // Debug.Log("pushed mesh data in: " + sw.Elapsed.TotalMilliseconds + " ms");
            lineList.Dispose();
            fillList.Dispose();
            TypedUnsafe.Dispose(requests, Allocator.Temp);
            TypedUnsafe.Dispose(responses, Allocator.Temp);
            
            // Debug.Log("Total time: " + total.Elapsed.TotalMilliseconds + " ms");
        }

        private AscentNormalizedFontMetrics fontMetrics;

        [MonoPInvokeCallback(typeof(TextServer.DebugCallback))]
        static void DebugLog(string message) {
            Debug.Log(message);
        }
        
        public void Start() {

            if (textureSize == Vector2.zero) {
                textureSize = new Vector2(1024, 2048);
            }

            // ctx = TextServer.CreateRenderContext(DebugLog);

            string font = Path.Combine(UnityEngine.Application.dataPath, "Fonts/Roboto-Regular.ttf");
            AscentNormalizedFontMetrics m;
            fontId = TextServer.RegisterFontFile(ctx, font, &m);
            fontMetrics = m;

            totalCodepoints = TextServer.GetGlyphCount(ctx, fontId);
            codepointList = new uint[totalCodepoints];
            fixed (uint* pCodepoints = codepointList) {
                TextServer.GetGlyphCodepoints(ctx, fontId, pCodepoints);
            }
        }

        public void OnApplicationQuit() {
            // TextServer.DestroyRenderContext(ctx);
        }

        public void OnPostRender() {

            if (renderedCodepoints == 0) {
                renderedCodepoints = 500;
            }

            if (renderStart > renderedCodepoints) {
                renderStart = renderedCodepoints - 1;
            }

            if (renderStart < 0) {
                renderStart = 0;
            }

            if (lastCodepoints != renderedCodepoints || renderStart != lastRenderStart) {
                lastRenderStart = renderStart;
                lastCodepoints = renderedCodepoints;
                UpdateRender(renderedCodepoints);
            }

            textCamera.orthographic = true;
            textCamera.orthographicSize = Screen.height * 0.5f;
            textCamera.backgroundColor = Color.black;

            Vector3 position = textCamera.transform.position;
            textCamera.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, -2f);

            lineMaterial.SetPass(0);
            Graphics.DrawMeshNow(lineMesh, Matrix4x4.Translate(translation));
            fillMaterial.SetPass(0);

            fillMaterial.SetInt("_CullFace", (int) (CullMode.Back));
            fillMaterial.SetInt("_StencilPassOp", (int) StencilOp.IncrementSaturate);
            fillMaterial.SetInt("_StencilZFailOp", (int) StencilOp.IncrementSaturate);
            fillMaterial.SetPass(0);

            Graphics.DrawMeshNow(fillMesh, Matrix4x4.Translate(translation));

            fillMaterial.SetInt("_CullFace", (int) (CullMode.Front));
            fillMaterial.SetInt("_StencilPassOp", (int) StencilOp.DecrementSaturate);
            fillMaterial.SetInt("_StencilZFailOp", (int) StencilOp.DecrementSaturate);
            fillMaterial.SetPass(0);

            Graphics.DrawMeshNow(fillMesh, Matrix4x4.Translate(translation));

            blendMaterial.SetPass(0);
            Graphics.DrawMeshNow(fullScreenQuad, Matrix4x4.identity);

            textCamera.transform.position = position;

        }

    }

}