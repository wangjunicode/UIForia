using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Batch = UIForia.Graphics.Batch;

namespace UIForia.Graphics {

    [StructLayout(LayoutKind.Explicit)]
    public struct DrawSortId {

        [FieldOffset(0)] public int sortId;
        [FieldOffset(0)] public ushort localRenderIdx;
        [FieldOffset(2)] public ushort baseRenderIdx;

        public DrawSortId(ushort localRenderIdx, ushort baseRenderIdx) {
            this.sortId = 0;
            this.localRenderIdx = localRenderIdx;
            this.baseRenderIdx = baseRenderIdx;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IndirectArg {

        public int indexCountPerInstance;
        public int instanceCount;
        public int startIndexLocation;
        public int baseVertexLocation;
        public int startInstanceLocation;

    }

    [Flags]
    public enum DrawType2 {

        UIForiaElement = 1 << 0,
        UIForiaText = 1 << 1,

        PushClipScope = 1 << 2,
        PushStencilClip = 1 << 3,
        PushClipRect = 1 << 4,
        PopClipper = 1 << 5,
        Callback = 1 << 6,

        BeginStencilClip = 1 << 7,
        PopClipRect = 1 << 8,
        UIForiaShadow = 1 << 9,

        Mesh2D = 1 << 10,
        Mesh3D = 1 << 11,

        // I'm not totally sure what these are yet
        UIForiaGeometry = 1 << 12,
        UIForiaSDF = 1 << 13,
        PopStencilClip = 1 << 14,

        PushRenderTargetRegion = 1 << 15,
        PopRenderTargetRegion = 1 << 16,

    }

    [DebuggerDisplay("{GetDebugDisplay()}")]
    public unsafe struct DrawInfo2 : IComparable<DrawInfo2> {

        public ElementId elementId; // todo -- temp

        public DrawSortId drawSortId;
        public DrawType2 drawType;
        public AxisAlignedBounds2D localBounds;
        public MaterialId materialId;
        public float4x4* matrix;
        public void* shapeData;
        public void* materialData;
        public MaterialPropertyOverride* materialOverrideValues;
        public int materialOverrideCount;

        public int CompareTo(DrawInfo2 other) {
            return drawSortId.sortId - other.drawSortId.sortId;
        }

        public string GetDebugDisplay() {
            string retn = " ElementId = " + elementId.index;
            retn += " -- " + drawType;
            return retn;
        }

        public bool IsNonRendering() {
            return (drawType == DrawType2.PushClipRect ||
                    drawType == DrawType2.PushClipScope ||
                    drawType == DrawType2.PushStencilClip ||
                    drawType == DrawType2.PopClipper ||
                    drawType == DrawType2.PopStencilClip ||
                    drawType == DrawType2.PopClipRect ||
                    drawType == DrawType2.Callback);
        }

        public bool IsInternalDrawType() {
            return (drawType == DrawType2.UIForiaText ||
                    drawType == DrawType2.UIForiaElement ||
                    drawType == DrawType2.UIForiaShadow ||
                    drawType == DrawType2.UIForiaGeometry ||
                    drawType == DrawType2.UIForiaSDF);
        }

    }

    public struct ElementMaterialSetup {

        public TextureUsage bodyTexture;
        public TextureUsage outlineTexture;
        public TextureUsage maskTexture;

    }

    public struct TextMaterialSetup {

        public TextureUsage faceTexture;
        public TextureUsage outlineTexture;
        public int fontTextureId;
        public bool requiresMaterialScan;
        public int defaultMaterialIdx;

    }

    // note -- it is important that these stay aligned on sizeof(float4) boundaries
    [AssertSize(32)]
    [StructLayout(LayoutKind.Sequential)]
    public struct UIForiaVertex {

        public float2 position;
        public float2 texCoord0;
        public uint4 indices;

    }

    internal struct CommandBufferCallback {

        public object context;
        public Action<object, CommandBuffer> callback;

    }

}

namespace UIForia.Systems {

    public unsafe class RenderSystem2 : IRenderSystem, IDisposable {

        public event Action<RenderContext> DrawDebugOverlay;

        private Application application;
        private LayoutSystem layoutSystem;
        private ElementSystem elementSystem;
        private UnsafeData unsafeData;
        private RenderBoxPool renderBoxPool;
        private ResourceManager resourceManager;
        private MaterialPropertyBlock mpb;
        private RenderContext3 renderContext;

        private RenderBox[] renderBoxTable;
        private int elementCapacity;
        private JobHandle renderHandle;

        private GraphicsBuffer[] indexBuffers;

        private ComputeBuffer glyphBuffer;
        private ComputeBuffer fontBuffer;
        private bool needsFontUpload;

        private ComputeBuffer[] vertexBuffers;
        private ComputeBuffer[] matrixBuffers;
        private ComputeBuffer[] float4Buffers;
        private ComputeBuffer[] materialBuffers;
        private ComputeBuffer[] indirectArgBuffers;

        private int frameId;
        private bool doubleBuffer;
        private UnmanagedRenderContext unmanagedRenderContext;

        public RenderSystem2(Application application, LayoutSystem layoutSystem, ElementSystem elementSystem) {
            this.elementSystem = elementSystem;
            this.layoutSystem = layoutSystem;
            this.application = application;
            this.renderBoxPool = new RenderBoxPool(application.ResourceManager);
            this.resourceManager = application.ResourceManager;
            this.mpb = new MaterialPropertyBlock();
            this.renderContext = new RenderContext3(resourceManager);

            this.glyphBuffer = new ComputeBuffer(512, sizeof(GPUGlyphInfo), ComputeBufferType.Structured); // todo -- try as constant
            this.fontBuffer = new ComputeBuffer(8, sizeof(GPUFontInfo), ComputeBufferType.Structured); // todo -- try as constant
            
            this.vertexBuffers = new ComputeBuffer[2];
            this.matrixBuffers = new ComputeBuffer[2];
            this.float4Buffers = new ComputeBuffer[2];
            this.materialBuffers = new ComputeBuffer[2];
            this.indirectArgBuffers = new ComputeBuffer[2];
            this.indexBuffers = new GraphicsBuffer[2];

            fontBuffer.name = "UIForia::FontBuffer";
            glyphBuffer.name = "UIForia::GlyphBuffer";
            unsafeData.Initialize(this);
            resourceManager.onFontAdded += HandleFontAdded;

            needsFontUpload = true;
            doubleBuffer = true;
            unmanagedRenderContext.Initialize();
        }

        private void HandleFontAdded(FontAsset fontAsset) {
            needsFontUpload = true;
        }

        public void SetCamera(Camera camera) { }

        public void Dispose() {

            vertexBuffers[0]?.Dispose();
            vertexBuffers[1]?.Dispose();

            matrixBuffers[0]?.Dispose();
            matrixBuffers[1]?.Dispose();

            materialBuffers[0]?.Dispose();
            materialBuffers[1]?.Dispose();

            float4Buffers[0]?.Dispose();
            float4Buffers[1]?.Dispose();

            indirectArgBuffers[0]?.Dispose();
            indirectArgBuffers[1]?.Dispose();

            indexBuffers[0]?.Dispose();
            indexBuffers[1]?.Dispose();

            glyphBuffer.Dispose();
            fontBuffer.Dispose();

            unsafeData.Dispose();
        }

        public virtual void OnUpdate() {
            frameId++;
            unsafeData.Clear();
            renderContext.Clear();
            unsafeData.float4Buffer.size = 0;

            resourceManager.GetFontTextures(renderContext.textureMap);
            List<UIView> views = layoutSystem.application.views;

            unsafeData.perViewElementList.SetSize(views.Count);

            for (int i = 0; i < views.Count; i++) {

                LayoutContext layoutContext = layoutSystem.GetLayoutContext(views[i]);

                unsafeData.perViewElementList[i] = new ElementList() {
                    size = layoutContext.elementList.size,
                    array = layoutContext.elementList.GetArrayPointer()
                };

            }

            //
            // UIForiaScheduler.Run(new GatherRenderedElements() {
            //     elementLists = unsafeData.perViewElementList,
            //     traversalTable = elementSystem.traversalTable,
            //     clipInfoTable = layoutSystem.clipInfoTable,
            //     renderCallList = unsafeData.renderCallList,
            //     renderInfoTable = unsafeData.renderInfoTable,
            // });
            new GatherRenderedElements() {
                elementLists = unsafeData.perViewElementList,
                traversalTable = elementSystem.traversalTable,
                clipInfoTable = layoutSystem.clipInfoTable,
                renderCallList = unsafeData.renderCallList,
                renderInfoTable = unsafeData.renderInfoTable,
            }.Execute();
            // todo -- if i end up using native painters, will need to include them in draw list
            RunMainThreadPainters();

            unsafeData.drawList.AddRange(renderContext.drawList.GetArrayPointer(), renderContext.drawList.size);

            new SortDrawInfoList() {drawList = unsafeData.drawList}.Run();

            // if (!printed) {
            //     printed = true;
            //     for (int i = 0; i < unsafeData.drawList.size; i++) {
            //         var depth = elementSystem.instanceTable[unsafeData.drawList[i].elementId.index].hierarchyDepth;
            //         Debug.Log(new string(' ', 4 * depth) + " " + elementSystem.instanceTable[unsafeData.drawList[i].elementId.index] + "   " + unsafeData.drawList[i].drawType);
            //     }
            // }

            unsafeData.transformedBounds.SetSize(unsafeData.drawList.size);
            unsafeData.materialPermutationIds.SetSize(unsafeData.drawList.size);
            unsafeData.matrixIdList.SetSize(unsafeData.drawList.size);
            unsafeData.materialIdList.SetSize(unsafeData.drawList.size);
            unsafeData.meshInfoList.SetSize(unsafeData.drawList.size);
            unsafeData.clipRectIdList.SetSize(unsafeData.drawList.size);
            unsafeData.renderTraversalList.SetSize(unsafeData.drawList.size);

            // these need full clears because not every drawInfo in the list is renderable
            // and we don't write data for them in that case. This makes sure we have a clean slate
            unsafeData.renderTraversalList.Clear();
            unsafeData.meshInfoList.Clear();

            // this must fill first because I need to combine clipBuffer with float4 buffer
            // or clip buffer gets bound as cbuffer, unlikely to need 4096 rect clippers...right?

            UIForiaScheduler.Run(new ProcessClipping() {
                drawList = unsafeData.drawList,
                transformedBounds = unsafeData.transformedBounds,
                // todo -- verify this should really be dpi scaled
                surfaceWidth = application.Width,
                surfaceHeight = application.Height,
                clipRectIdList = unsafeData.clipRectIdList,
                clipperBoundsList = unsafeData.clipRectBuffer,
                clipRenderList = unsafeData.renderTraversalList,
                stencilDataList = unsafeData.stencilDataList

            });

            // SYNCHRONOUS!!!!
            unsafeData.float4Buffer.AddRange((float4*) unsafeData.clipRectBuffer.GetArrayPointer(), unsafeData.clipRectBuffer.size);

            NativeArray<JobHandle> preProcessHandles = new NativeArray<JobHandle>(4, Allocator.Temp);
            NativeArray<JobHandle> shapeBakingHandles = new NativeArray<JobHandle>(3, Allocator.Temp);

            // this needs to happen either after text, or output to a separate material buffer
            shapeBakingHandles[0] = UIForiaScheduler.Run(new BakeUIForiaElements() {
                // need a separate float buffer in order to run in parallel with text
                // can just bind a separate one since they never render at the same time and we swap shader anyway
                // then another shared clip buffer? or copy clip buffer into each? 
                // or just loop & offset element one, would like a single clip buffer for lots of reasons
                drawList = unsafeData.drawList,
                float4Buffer = unsafeData.float4Buffer,
                vertexList = unsafeData.elementVertexList,
                materialList = unsafeData.elementMaterialBuffer,
                meshInfoList = unsafeData.meshInfoList
            });

            // might want to do this after culling finishes
            // otherwise I'm uploading data for potentially culled render groups
            shapeBakingHandles[1] = UIForiaScheduler.Run(new BakeUIForiaText() {
                drawList = unsafeData.drawList,
                vertexList = unsafeData.textVertexList,
                meshInfoList = unsafeData.meshInfoList,
                materialBuffer = unsafeData.materialBuffer,
                float4Buffer = unsafeData.float4Buffer,
                textEffectBuffer = application.textSystem.textEffectVertexInfoTable
            });

            preProcessHandles[0] = UIForiaScheduler.Run(new BuildMaterialPermutations() {
                drawList = unsafeData.drawList,
                permutationList = unsafeData.materialPermutationsList,
                materialPermutationIdList = unsafeData.materialPermutationIds,
            });

            preProcessHandles[1] = UIForiaScheduler.Run(new BuildMatrixBuffer() {
                drawList = unsafeData.drawList,
                matrixBuffer = unsafeData.matrixBuffer,
                matrixIndices = unsafeData.matrixIdList
            });

            // requires clipping matrix building to be complete
            new CombineUIForiaVertexBuffers() {
                textMaterialBuffer = unsafeData.materialBuffer,
                elementMaterialBuffer = unsafeData.elementMaterialBuffer,
                textVertexList = unsafeData.textVertexList,
                elementVertexList = unsafeData.elementVertexList,
                meshList = unsafeData.meshInfoList,
                matrixIdList = unsafeData.matrixIdList,
                clipRectIdList = unsafeData.clipRectIdList
            }.Run();

            new CreateRenderPasses() {
                drawList = unsafeData.drawList,
                batchList = unsafeData.batchList,
                batchMemberList = unsafeData.batchMemberList,
                materialPermutationIds = unsafeData.materialPermutationIds,
                materialPermutations = unsafeData.materialPermutationsList,
                renderCommands = unsafeData.renderCommands,
                stencilList = unsafeData.stencilDataList,
                clipperBoundsList = unsafeData.clipRectBuffer,
                renderTraversalList = unsafeData.renderTraversalList,
                transformedBounds = unsafeData.transformedBounds,
            }.Run();

            new BuildIndexBuffer() {
                indirectArgs = unsafeData.indirectArgBuffer,
                meshInfoList = unsafeData.meshInfoList,
                batchList = unsafeData.batchList,
                batchMemberList = unsafeData.batchMemberList,
                indexBuffer = unsafeData.uiforiaIndexBuffer,
            }.Run();

            // todo -- could be done later, will still need to upload meshes n stuff

            renderHandle.Complete();

            preProcessHandles.Dispose();
            shapeBakingHandles.Dispose();

        }

        private static readonly int s_VertexBufferKey = Shader.PropertyToID("_UIForiaVertexBuffer");
        private static readonly int s_MatrixBufferKey = Shader.PropertyToID("_UIForiaMatrixBuffer");
        private static readonly int s_Float4BufferKey = Shader.PropertyToID("_UIForiaFloat4Buffer");
        private static readonly int s_MaterialBufferKey = Shader.PropertyToID("_UIForiaMaterialBuffer");
        private static readonly int s_OriginMatrixKey = Shader.PropertyToID("_UIForiaOriginMatrix");
        private static readonly int s_FontDataBufferKey = Shader.PropertyToID("_UIForiaFontBuffer");
        private static readonly int s_GlyphBufferKey = Shader.PropertyToID("_UIForiaGlyphBuffer");
        private static readonly int s_DPIScaleFactorKey = Shader.PropertyToID("_UIForiaDPIScale");

        private static readonly int s_MainTextureKey = Shader.PropertyToID("_MainTex");
        private static readonly int s_FontTextureKey = Shader.PropertyToID("_FontTexture");
        private static readonly int s_MaskTextureKey = Shader.PropertyToID("_MaskTexture");
        private static readonly int s_OutlineTextureKey = Shader.PropertyToID("_OutlineTex");

        private void UpdateComputeBufferData<T>(DataList<T>.Shared list, string bufferName, ComputeBuffer[] buffers, int idx, ComputeBufferType bufferType = ComputeBufferType.Structured) where T : unmanaged {
            ComputeBuffer buffer = buffers[idx];
            if (ReferenceEquals(buffer, null) || list.size > buffer.count) {
                buffer?.Release();
                buffer?.Dispose();
                buffer = new ComputeBuffer(list.capacity, sizeof(T), bufferType);
                buffer.name = bufferName + idx;
                buffers[idx] = buffer;
            }

            NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                list.GetArrayPointer(),
                list.size,
                Allocator.None
            );
#if UNITY_EDITOR
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(unsafeData.dummyArray));
#endif

            buffers[idx].SetData(array, 0, 0, list.size);

        }

        public void Render(float surfaceWidth, float surfaceHeight, CommandBuffer commandBuffer) {
            // renderHandle.Complete();
            Profiler.BeginSample("UIForia::ExecuteDraw");
            int bufferIdx = frameId % 2;
            if (!doubleBuffer) {
                bufferIdx = 0;
            }

            if (needsFontUpload) {
                needsFontUpload = false;

                unsafeData.glyphBuffer.size = 0;
                unsafeData.glyphBuffer.EnsureCapacity(resourceManager.renderedCharacterInfoList.size);
                unsafeData.glyphBuffer.AddRange(resourceManager.renderedCharacterInfoList.GetArrayPointer(), resourceManager.renderedCharacterInfoList.size);

                unsafeData.fontBuffer.size = 0;
                unsafeData.fontBuffer.EnsureCapacity(resourceManager.gpuFontInfoList.size);
                unsafeData.fontBuffer.AddRange(resourceManager.gpuFontInfoList.GetArrayPointer(), resourceManager.gpuFontInfoList.size);

                if (glyphBuffer == null || glyphBuffer.count < unsafeData.glyphBuffer.size) {
                    glyphBuffer?.Dispose();
                    glyphBuffer = new ComputeBuffer(unsafeData.glyphBuffer.capacity, sizeof(GPUGlyphInfo), ComputeBufferType.Structured);
                }

                if (fontBuffer == null || fontBuffer.count < unsafeData.fontBuffer.size) {
                    fontBuffer?.Dispose();
                    fontBuffer = new ComputeBuffer(unsafeData.fontBuffer.capacity, sizeof(GPUFontInfo), ComputeBufferType.Structured);
                }

                NativeArray<GPUGlyphInfo> glyphArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<GPUGlyphInfo>(
                    unsafeData.glyphBuffer.GetArrayPointer(),
                    unsafeData.glyphBuffer.size,
                    Allocator.None
                );

                NativeArray<GPUFontInfo> fontArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<GPUFontInfo>(
                    unsafeData.fontBuffer.GetArrayPointer(),
                    unsafeData.fontBuffer.size,
                    Allocator.None
                );

#if UNITY_EDITOR
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref glyphArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(unsafeData.dummyArray));
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref fontArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(unsafeData.dummyArray));
#endif
                glyphBuffer.SetData(glyphArray, 0, 0, unsafeData.glyphBuffer.size);
                fontBuffer.SetData(fontArray, 0, 0, unsafeData.fontBuffer.size);

            }

            Profiler.BeginSample("Set UIForia Render Data");
            UpdateComputeBufferData(unsafeData.materialBuffer, "UIForia::MaterialBuffer", materialBuffers, bufferIdx);
            UpdateComputeBufferData(unsafeData.textVertexList, "UIForia::VertexBuffer", vertexBuffers, bufferIdx);
            // todo -- convert to float3x2 to save on upload cost in 2d case, and maybe a quat, vec3, vec3 for 3d?
            UpdateComputeBufferData(unsafeData.matrixBuffer, "UIForia::MatrixBuffer", matrixBuffers, bufferIdx);
            UpdateComputeBufferData(unsafeData.float4Buffer, "UIForia::Float4Buffer", float4Buffers, bufferIdx);
            UpdateComputeBufferData(unsafeData.indirectArgBuffer, "UIForia::IndirectArgBuffer", indirectArgBuffers, bufferIdx, ComputeBufferType.IndirectArguments);
            Profiler.EndSample();

            ComputeBuffer vertexBuffer = vertexBuffers[bufferIdx];
            ComputeBuffer matrixBuffer = matrixBuffers[bufferIdx];
            ComputeBuffer materialBuffer = materialBuffers[bufferIdx];
            ComputeBuffer float4Buffer = float4Buffers[bufferIdx];
            ComputeBuffer argBuffer = indirectArgBuffers[bufferIdx];
            GraphicsBuffer indexBuffer = indexBuffers[bufferIdx];

            if (ReferenceEquals(indexBuffer, null) || unsafeData.uiforiaIndexBuffer.size > indexBuffer.count) {
                indexBuffer?.Dispose();
                indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, unsafeData.uiforiaIndexBuffer.capacity, sizeof(int));
                indexBuffers[bufferIdx] = indexBuffer;
            }

            NativeArray<int> triangleArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(
                unsafeData.uiforiaIndexBuffer.GetArrayPointer(),
                unsafeData.uiforiaIndexBuffer.size,
                Allocator.None
            );

#if UNITY_EDITOR
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref triangleArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(unsafeData.dummyArray));
#endif

            indexBuffer.SetData(triangleArray, 0, 0, unsafeData.uiforiaIndexBuffer.size);

            RenderCommand* renderCommands = unsafeData.renderCommands.GetArrayPointer();
            int renderCommandCount = unsafeData.renderCommands.size;

            float halfWidth = surfaceWidth * 0.5f;
            float halfHeight = surfaceHeight * 0.5f;

            Matrix4x4 origin = Matrix4x4.TRS(new Vector3(-halfWidth, halfHeight, 0), Quaternion.identity, Vector3.one * application.DPIScaleFactor);

            Matrix4x4 identity = Matrix4x4.identity;

            RenderTexture baseTexture = RenderTexture.active;

            commandBuffer.SetViewProjectionMatrices(identity, Matrix4x4.Ortho(-halfWidth, halfWidth, -halfHeight, halfHeight, -100, 100));

            commandBuffer.SetGlobalBuffer(s_MatrixBufferKey, matrixBuffer);
            commandBuffer.SetGlobalBuffer(s_VertexBufferKey, vertexBuffer);
            commandBuffer.SetGlobalBuffer(s_Float4BufferKey, float4Buffer);
            commandBuffer.SetGlobalBuffer(s_MaterialBufferKey, materialBuffer);
            commandBuffer.SetGlobalBuffer(s_FontDataBufferKey, fontBuffer);
            commandBuffer.SetGlobalBuffer(s_GlyphBufferKey, glyphBuffer);

            commandBuffer.SetGlobalMatrix(s_OriginMatrixKey, origin);
            commandBuffer.SetGlobalFloat(s_DPIScaleFactorKey, application.DPIScaleFactor);

            for (int i = 0; i < renderCommandCount; i++) {

                ref RenderCommand renderCommand = ref renderCommands[i];
                switch (renderCommand.type) {

                    case RenderCommandType.ShadowBatch: {

                        mpb.Clear();

                        Batch batch = unsafeData.batchList[renderCommand.batchIndex];

                        Material material = resourceManager.GetMaterialInstance(MaterialId.UIForiaShadow);

                        SetupStencilState(batch, commandBuffer);

                        commandBuffer.DrawProceduralIndirect(indexBuffer, identity, material, 0, MeshTopology.Triangles, argBuffer, sizeof(IndirectArg) * batch.indirectArgOffset, mpb);
                        break;
                    }

                    case RenderCommandType.ElementBatch: {

                        mpb.Clear();

                        Batch batch = unsafeData.batchList[renderCommand.batchIndex];

                        Material material = resourceManager.GetMaterialInstance(MaterialId.UIForiaShape);

                        bool needsMpb = false;

                        if (batch.materialPermutation.texture0 != 0 && renderContext.textureMap.TryGetValue(batch.materialPermutation.texture0, out Texture texture)) {
                            needsMpb = true;
                            mpb.SetTexture(s_MainTextureKey, texture);
                        }

                        if (batch.materialPermutation.texture1 != 0 && renderContext.textureMap.TryGetValue(batch.materialPermutation.texture1, out texture)) {
                            needsMpb = true;
                            mpb.SetTexture(s_OutlineTextureKey, texture);
                        }

                        if (batch.materialPermutation.texture2 != 0 && renderContext.textureMap.TryGetValue(batch.materialPermutation.texture2, out texture)) {
                            needsMpb = true;
                            mpb.SetTexture(s_MaskTextureKey, texture);
                        }

                        SetupStencilState(batch, commandBuffer);

                        commandBuffer.DrawProceduralIndirect(indexBuffer, identity, material, 0, MeshTopology.Triangles, argBuffer, sizeof(IndirectArg) * batch.indirectArgOffset, needsMpb ? mpb : null);

                        break;
                    }

                    case RenderCommandType.ShapeEffectBatch:
                        break;

                    case RenderCommandType.Mesh: {

                        Batch batch = unsafeData.batchList[renderCommand.batchIndex];

                        if (!renderContext.meshMap.TryGetValue(batch.meshId, out Mesh mesh)) {
                            continue;
                        }

                        if (!renderContext.materialMap.TryGetValue(batch.materialId.index, out Material material)) {
                            continue;
                        }

                        mpb.Clear();
                        //SetupMaterialProperties(batch);

                        // SetupStencilState(batch, commandBuffer); todo -- borked
                        commandBuffer.DrawMesh(mesh, math.mul(origin, *batch.matrix), material, 0, 0, mpb);
                        break;
                    }

                    case RenderCommandType.MeshBatch:
                        break;

                    case RenderCommandType.SDFTextBatch: {
                        mpb.Clear();

                        Batch batch = unsafeData.batchList[renderCommand.batchIndex];

                        Material material = resourceManager.GetMaterialInstance(MaterialId.UIForiaSDFText);

                        if (batch.materialPermutation.texture0 != 0 && renderContext.textureMap.TryGetValue(batch.materialPermutation.texture0, out Texture texture)) {
                            mpb.SetTexture(s_MainTextureKey, texture);
                        }

                        if (batch.materialPermutation.texture1 != 0 && renderContext.textureMap.TryGetValue(batch.materialPermutation.texture1, out texture)) {
                            mpb.SetTexture(s_OutlineTextureKey, texture);
                        }

                        if (batch.materialPermutation.texture2 != 0 && renderContext.textureMap.TryGetValue(batch.materialPermutation.texture2, out texture)) {
                            mpb.SetTexture(s_FontTextureKey, texture);
                        }

                        SetupStencilState(batch, commandBuffer);
                        commandBuffer.DrawProceduralIndirect(indexBuffer, identity, material, 0, MeshTopology.Triangles, argBuffer, sizeof(IndirectArg) * batch.indirectArgOffset, mpb);

                        break;
                    }

                    case RenderCommandType.SDFTextEffectBatch:
                        break;

                    case RenderCommandType.CreateRenderTarget:
                        break;

                    case RenderCommandType.PushRenderTexture:
                        RenderTexture rt = renderContext.renderTextures[renderCommand.batchIndex];
                        // todo -- need to offset newOrigin by the screen xy of the element providing it
                        Matrix4x4 newOrigin = Matrix4x4.TRS(new Vector3(-(rt.width / 2f) - 10, (rt.height / 2f) + 10, 0), Quaternion.identity, Vector3.one * application.DPIScaleFactor);
                        commandBuffer.SetRenderTarget(rt);
                        commandBuffer.ClearRenderTarget(true, true, Color.clear);
                        commandBuffer.SetGlobalMatrix(s_OriginMatrixKey, newOrigin);
                        commandBuffer.SetViewProjectionMatrices(identity, Matrix4x4.Ortho(-(rt.width / 2f), rt.width / 2f, -(rt.height / 2f), rt.height / 2f, -100, 100));
                        break;

                    case RenderCommandType.PopRenderTexture:
                        commandBuffer.SetRenderTarget(baseTexture);
                        commandBuffer.SetGlobalMatrix(s_OriginMatrixKey, origin);
                        commandBuffer.SetViewProjectionMatrices(identity, Matrix4x4.Ortho(-halfWidth, halfWidth, -halfHeight, halfHeight, -100, 100));
                        break;

                    case RenderCommandType.ClearRenderTarget:
                        break;

                    case RenderCommandType.MaskAtlasBatch:
                        break;

                    case RenderCommandType.UpdateClipRectBuffer:
                        break;

                    case RenderCommandType.SetClipRectBuffer:
                        break;

                    case RenderCommandType.Callback:
                        break;

                    case RenderCommandType.SetTextDataBuffer:
                        break;

                    case RenderCommandType.SetShapeDatabuffer:
                        break;

                    case RenderCommandType.SetGradientDataBuffer:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }

            Profiler.EndSample();
        }

        private static readonly int k_ColorMaskKey = Shader.PropertyToID("_UIForiaColorMask");
        private static readonly int k_StencilRefKey = Shader.PropertyToID("_UIForiaStencilRef");
        private static readonly int k_StencilCompKey = Shader.PropertyToID("_UIForiaStencilComp");
        private static readonly int k_StencilOpKey = Shader.PropertyToID("_UIForiaStencilOp");

        // todo -- only set if changed from last call
        private StencilType stencilType;
        private ColorWriteMask colorWriteMask;
        private int refValue;

        private void SetupStencilState(in Batch batch, CommandBuffer commandBuffer) {
            bool update = (batch.stencilType != stencilType || refValue != batch.stencilRefValue || batch.colorMask != colorWriteMask);

//            if (!update) {
//                return;
//            }

            if (batch.stencilType == StencilType.Push) {
                commandBuffer.SetGlobalInt(k_ColorMaskKey, (int) batch.colorMask);
                commandBuffer.SetGlobalInt(k_StencilRefKey, batch.stencilRefValue);
                commandBuffer.SetGlobalInt(k_StencilCompKey, (int) CompareFunction.Equal);
                commandBuffer.SetGlobalInt(k_StencilOpKey, (int) StencilOp.IncrementSaturate);
            }
            else if (batch.stencilType == StencilType.Pop) {
                commandBuffer.SetGlobalInt(k_ColorMaskKey, (int) batch.colorMask);
                commandBuffer.SetGlobalInt(k_StencilRefKey, batch.stencilRefValue);
                commandBuffer.SetGlobalInt(k_StencilCompKey, (int) CompareFunction.Equal);
                commandBuffer.SetGlobalInt(k_StencilOpKey, (int) StencilOp.DecrementSaturate);
            }
            else if (batch.stencilType == StencilType.Ignore) {
                commandBuffer.SetGlobalInt(k_ColorMaskKey, (int) batch.colorMask);
                commandBuffer.SetGlobalInt(k_StencilRefKey, batch.stencilRefValue);
                commandBuffer.SetGlobalInt(k_StencilCompKey, (int) CompareFunction.Always);
                commandBuffer.SetGlobalInt(k_StencilOpKey, (int) StencilOp.Keep);
            }
            else {
                commandBuffer.SetGlobalInt(k_ColorMaskKey, (int) batch.colorMask);
                commandBuffer.SetGlobalInt(k_StencilRefKey, batch.stencilRefValue);
                commandBuffer.SetGlobalInt(k_StencilCompKey, (int) CompareFunction.Equal);
                commandBuffer.SetGlobalInt(k_StencilOpKey, (int) StencilOp.Keep);
            }
        }

        public struct UnmanagedRenderCall {

            public int renderId;
            public int renderOp;
            public bool isElement;
            public int backgroundTexture;
            public int outlineTexture;
            public ElementDrawDesc drawDesc;

        }

        private void RunMainThreadPainters() {
            Profiler.BeginSample("Run Managed Painters");
            DataList<RenderCallInfo>.Shared list = unsafeData.renderCallList;
            RenderCallInfo* array = list.GetArrayPointer();

            float4x4* matrices = layoutSystem.worldMatrices.array;
            ElementTable<ClipInfo> clipInfoTable = layoutSystem.clipInfoTable;

            int size = list.size;

            for (int i = 0; i < size; i++) {
                ref RenderCallInfo callInfo = ref array[i];
                ElementId elementId = callInfo.elementId;

                int idx = elementId.id & ElementId.ENTITY_INDEX_MASK;
                RenderBox box = renderBoxTable[idx];

                if (box == null) continue;

                MaterialId materialId = box.materialId;

                ref LayoutBoxInfo layoutResult = ref layoutSystem.layoutResultTable.array[idx];

                if (layoutResult.sizeChanged) {
                    layoutResult.sizeChanged = false;
                    box.OnSizeChanged(new Size(layoutResult.actualSize.x, layoutResult.actualSize.y));
                }

                // if (box.isBuiltIn) {
                //
                //     if (box.isElementBox) {
                //         StandardRenderBox2 box2 = (StandardRenderBox2) box;
                //         unsafeData.unmanagedRenderCalls.Add(new UnmanagedRenderCall() {
                //             renderOp = callInfo.renderOp,
                //             renderId = i,
                //             backgroundTexture = 0,
                //             outlineTexture = 0,
                //             drawDesc = box2.drawDesc
                //         });
                //     }
                //     else {
                //         unsafeData.unmanagedRenderCalls.Add(new UnmanagedRenderCall() {
                //             renderOp = callInfo.renderOp,
                //             renderId = i,
                //             backgroundTexture = 0,
                //             outlineTexture = 0,
                //             drawDesc = default,
                //         });
                //     }
                //
                //     continue;
                // }

                if (callInfo.renderOp == 0) {

                    // todo -- if clipper via styles, set that up here
                    renderContext.Setup(elementId, materialId, i, matrices + idx);
                    // todo -- if is clipper, push clip rect or stencil, probably after the draw
                    box.PaintBackground3(renderContext);
                }
                else {

                    // todo -- if clipper via styles, tear it down here
                    renderContext.Setup(elementId, materialId, i, matrices + idx);
                    box.PaintForeground3(renderContext);
                }

            }

            Profiler.EndSample();
        }

        private void ResizeBackingStore(int newCapacity) {

            Array.Resize(ref renderBoxTable, newCapacity * 2);

            RenderInfo* ptr = TypedUnsafe.Malloc<RenderInfo>(newCapacity * 2, Allocator.Persistent);
            if (unsafeData.renderInfoTable.array != null) {
                TypedUnsafe.MemCpy(ptr, unsafeData.renderInfoTable.array, elementCapacity);
                TypedUnsafe.Dispose(unsafeData.renderInfoTable.array, Allocator.Persistent);
            }

            unsafeData.renderInfoTable.array = ptr;
            elementCapacity = newCapacity * 2;
        }

        public void HandleElementsEnabled(DataList<ElementId>.Shared enabledElements) {

            int maxIndex = enabledElements[0].index;

            for (int i = 0; i < enabledElements.size; i++) {
                if (enabledElements[i].index > maxIndex) {
                    maxIndex = enabledElements[i].index;
                }
            }

            if (maxIndex >= elementCapacity) {
                ResizeBackingStore(maxIndex);
            }

            for (int i = 0; i < enabledElements.size; i++) {

                ElementId elementId = enabledElements[i];

                UIElement instance = elementSystem.instanceTable[elementId.index];

                // 2 options
                // 1 - always recycle and renew painters
                // saves memory potentially
                // makes lifecycle harder and less predictable
                // only do this for build in ones is probably reasonable
                // 2 - always create new ones
                // bad for gc good for lifecycle

                string painter = instance.style.Painter;

                RenderBox renderBox = null;

                if (!string.IsNullOrEmpty(painter)) {
                    renderBox = renderBoxPool.GetCustomPainter(painter);
                }

                if (renderBox == null) {
                    if (instance is UITextElement) {
                        //if (instance.renderBox == null) {
                        renderBox = new TextRenderBox2();
                        //}
                    }
                    else if (instance is UIVideoElement) {
                        renderBox = new VideoRenderBox();
                    }
                    else {
                        renderBox = new StandardRenderBox2();
                    }
                }

                renderBoxTable[elementId.index] = renderBox;

                unsafeData.renderInfoTable[elementId] = new RenderInfo() {
                    layer = instance.style.Layer,
                    zIndex = instance.style.ZIndex,
                };

                instance.renderBox = renderBox;
                renderBox.element = instance;
                instance.renderBox.OnInitialize();

            }

        }

        public void HandleElementsDisabled(DataList<ElementId>.Shared disabledElements) {
            // todo -- implement this

            for (int i = 0; i < disabledElements.size; i++) {
                ElementId elementId = disabledElements[i];

                UIElement element = elementSystem.instanceTable[elementId.index];

                if (element.renderBox == null) {
                    continue;
                }

                if (elementSystem.IsAlive(elementId)) {
                    if (element.renderBox is TextRenderBox2 textRenderBox) { }
                    else if (element.renderBox is StandardRenderBox2 standardRenderBox) { }
                    else {
                        element.renderBox.OnDisable();
                    }
                }
                else {
                    if (element.renderBox is TextRenderBox2 textRenderBox) { }
                    else if (element.renderBox is StandardRenderBox2 standardRenderBox) { }
                    else {
                        element.renderBox?.OnDestroy();
                    }
                }

                element.renderBox = null;

            }

        }

        public void HandleStylePropertyUpdates(UIElement element, StyleProperty[] propertyList, int propertyCount) {
            if (element.renderBox == null) return;

            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref propertyList[i];
                switch (property.propertyId) {
                    case StylePropertyId.Painter:
                        // ReplaceRenderBox(element, property.AsString);
                        break;

                    case StylePropertyId.OverflowX:
                    case StylePropertyId.OverflowY:
                        break;
                }
            }

            element.renderBox.OnStylePropertyChanged(propertyList, propertyCount);
        }

        private struct UnsafeData : IDisposable {

            public DataList<DrawInfo2>.Shared drawList;
            public DataList<ElementList> perViewElementList;
            public DataList<RenderCallInfo>.Shared renderCallList;
            public ElementTable<RenderInfo> renderInfoTable;
            public DataList<AxisAlignedBounds2D> transformedBounds;
            public DataList<int>.Shared uiforiaIndexBuffer;
            public DataList<float4>.Shared float4Buffer;

            public DataList<RenderCommand>.Shared renderCommands;
            public DataList<Batch>.Shared batchList;
            public DataList<int>.Shared batchMemberList;
            public DataList<MaterialPermutation>.Shared materialPermutationsList;
            public DataList<float4x4>.Shared matrixBuffer;

            public List_Int32 materialPermutationIds;
            public List_Int32 matrixIdList;
            public List_Int32 clipRectIdList;
            public List_Int32 materialIdList;

            public DataList<AxisAlignedBounds2D>.Shared clipRectBuffer;
            public DataList<RenderTraversalInfo> renderTraversalList;
            public DataList<StencilInfo>.Shared stencilDataList;
            public DataList<UIForiaVertex>.Shared elementVertexList;
            public DataList<MeshInfo> meshInfoList;
            public DataList<UIForiaVertex>.Shared textVertexList;
            public DataList<UIForiaVertex>.Shared shapeVertexList;
            public DataList<int>.Shared shapeTriangleList;
            public DataList<UIForiaMaterialInfo>.Shared materialBuffer;

            public DataList<IndirectArg>.Shared indirectArgBuffer;
            public NativeArray<int> dummyArray;
            public DataList<GPUGlyphInfo> glyphBuffer;
            public DataList<GPUFontInfo> fontBuffer;
            public DataList<ElementMaterialInfo>.Shared elementMaterialBuffer;
            public DataList<UnmanagedRenderCall>.Shared unmanagedRenderCalls;

            public void Initialize(RenderSystem2 renderSystem2) {
                this.unmanagedRenderCalls = new DataList<UnmanagedRenderCall>.Shared(32, Allocator.Persistent);
                this.elementMaterialBuffer = new DataList<ElementMaterialInfo>.Shared(32, Allocator.Persistent);
                this.fontBuffer = new DataList<GPUFontInfo>(8, Allocator.Persistent);
                this.glyphBuffer = new DataList<GPUGlyphInfo>(512, Allocator.Persistent);
                this.indirectArgBuffer = new DataList<IndirectArg>.Shared(16, Allocator.Persistent);
                this.dummyArray = new NativeArray<int>(1, Allocator.Persistent);
                this.stencilDataList = new DataList<StencilInfo>.Shared(8, Allocator.Persistent);
                this.renderTraversalList = new DataList<RenderTraversalInfo>(64, Allocator.Persistent);
                this.renderCommands = new DataList<RenderCommand>.Shared(32, Allocator.Persistent);
                this.clipRectBuffer = new DataList<AxisAlignedBounds2D>.Shared(8, Allocator.Persistent);
                this.materialPermutationsList = new DataList<MaterialPermutation>.Shared(16, Allocator.Persistent);
                this.perViewElementList = new DataList<ElementList>(4, Allocator.Persistent);
                this.renderCallList = new DataList<RenderCallInfo>.Shared(64, Allocator.Persistent);
                this.drawList = new DataList<DrawInfo2>.Shared(64, Allocator.Persistent);
                this.transformedBounds = new DataList<AxisAlignedBounds2D>(64, Allocator.Persistent);
                this.matrixBuffer = new DataList<float4x4>.Shared(32, Allocator.Persistent);
                this.float4Buffer = new DataList<float4>.Shared(32, Allocator.Persistent);

                this.uiforiaIndexBuffer = new DataList<int>.Shared(1024, Allocator.Persistent);

                this.matrixIdList = new List_Int32(64, Allocator.Persistent);
                this.materialPermutationIds = new List_Int32(64, Allocator.Persistent);
                this.clipRectIdList = new List_Int32(64, Allocator.Persistent);
                this.batchList = new DataList<Batch>.Shared(16, Allocator.Persistent);
                this.batchMemberList = new DataList<int>.Shared(64, Allocator.Persistent);

                this.materialIdList = new List_Int32(64, Allocator.Persistent);
                this.materialBuffer = new DataList<UIForiaMaterialInfo>.Shared(64, Allocator.Persistent);
                this.elementVertexList = new DataList<UIForiaVertex>.Shared(4 * 64, Allocator.Persistent);
                this.textVertexList = new DataList<UIForiaVertex>.Shared(4 * 64, Allocator.Persistent);
                this.shapeVertexList = new DataList<UIForiaVertex>.Shared(16, Allocator.Persistent);
                this.shapeTriangleList = new DataList<int>.Shared(16, Allocator.Persistent);
                this.meshInfoList = new DataList<MeshInfo>(64, Allocator.Persistent);

            }

            public void Clear() {
                Profiler.BeginSample("Clear Render Setup");
                unmanagedRenderCalls.size = 0;
                renderCommands.size = 0;
                float4Buffer.size = 0;
                indirectArgBuffer.size = 0;
                uiforiaIndexBuffer.size = 0;
                elementMaterialBuffer.size = 0;
                materialBuffer.size = 0;
                shapeTriangleList.size = 0;
                shapeVertexList.size = 0;
                materialIdList.size = 0;
                textVertexList.size = 0;
                meshInfoList.size = 0;
                elementVertexList.size = 0;
                stencilDataList.size = 0;
                clipRectIdList.size = 0;
                clipRectBuffer.size = 0;
                matrixBuffer.size = 0;
                materialPermutationsList.size = 0;
                batchList.size = 0;
                batchMemberList.size = 0;
                drawList.size = 0;
                renderCallList.size = 0;
                perViewElementList.size = 0;
                materialPermutationIds.size = 0;
                matrixIdList.size = 0;
                transformedBounds.size = 0;
                renderTraversalList.size = 0;
                Profiler.EndSample();
            }

            public void Dispose() {
                fontBuffer.Dispose();
                unmanagedRenderCalls.Dispose();
                glyphBuffer.Dispose();
                meshInfoList.Dispose();
                dummyArray.Dispose();
                float4Buffer.Dispose();
                indirectArgBuffer.Dispose();
                materialIdList.Dispose();

                elementMaterialBuffer.Dispose();
                textVertexList.Dispose();
                elementVertexList.Dispose();

                materialBuffer.Dispose();
                stencilDataList.Dispose();
                renderTraversalList.Dispose();
                clipRectIdList.Dispose();
                clipRectBuffer.Dispose();
                matrixBuffer.Dispose();
                matrixIdList.Dispose();
                materialPermutationIds.Dispose();
                materialPermutationsList.Dispose();
                drawList.Dispose();
                renderCommands.Dispose();
                batchList.Dispose();
                batchMemberList.Dispose();
                uiforiaIndexBuffer.Dispose();

                perViewElementList.Dispose();
                renderCallList.Dispose();
                TypedUnsafe.Dispose(renderInfoTable.array, Allocator.Persistent);
                transformedBounds.Dispose();

            }

        }

        public void OnViewAdded(UIView view) { }

    }

}

// for mask generation

//     // Set BlendOp to min
//     // Set Blend params to One One
//     // Set ZWrite On
//     // Set ZTest to NotEqual
//     // set depth to # of parent masks
//     // draw parent shapes top down
//     // subtract 1 from depth each time
//     // draw child shape at depth 0
//     // draw a rect over the whole space
//     int start = vertexHelper.currentVertCount;
//     shapeKit.AddQuad(ref vertexHelper, 0, 0, 256, 256, Color.green);
//     for (int i = start; i < vertexHelper.currentVertCount; i++) {
//         vertexHelper.positions[i].z = -2f;
//     }
//
//     start = vertexHelper.currentVertCount;
//     shapeKit.AddQuad(ref vertexHelper, 32, 32, 256 - 48, 256 - 48, new Color32(0, 0, 0, 128));
//
//     for (int i = start; i < vertexHelper.currentVertCount; i++) {
//         vertexHelper.positions[i].z = -1f;
//     }
//     shapeKit.SetDpiScale(1);
//     shapeKit.SetAntiAliasWidth(0);//1.25f);
//     shapeKit.AddCircle(ref vertexHelper, new Rect(128, 128, 256, 256), new EllipseProperties() {
//         fitting = EllipseFitting.Ellipse,
//     }, Color.red);
//
//
//     //vertexHelper.Clear();
//     shapeKit.AddRect(ref vertexHelper, 0, 0, 256, 256, new Color32(0, 0, 0, 0));
//     vertexHelper.FillMesh(mesh);
//
// }
//