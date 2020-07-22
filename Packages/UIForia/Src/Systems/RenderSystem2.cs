using System;
using System.Collections.Generic;
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
        [FieldOffset(0)] public ushort baseRenderIdx;
        [FieldOffset(2)] public ushort localRenderIdx;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IndirectArg {

        public int indexCountPerInstance;
        public int instanceCount;
        public int startIndexLocation;
        public int baseVertexLocation;
        public int startInstanceLocation;

    }

    public struct DrawInfoComp : IComparer<DrawInfo2> {

        public int Compare(DrawInfo2 x, DrawInfo2 y) {
            return x.drawSortId.sortId - y.drawSortId.sortId;
        }

    }

    public enum DrawType2 {

        UIForiaElement,
        UIForiaText,

        // I'm not totally sure what these are yet
        UIForiaGeometry,
        UIForiaSDF,

        PushClipScope,
        PushStencilClip,
        PushClipRect,
        PopClipper,
        Callback,

        BeginStencilClip,

    }

    public unsafe struct DrawInfo2 {

        public DrawSortId drawSortId;

        public DrawType2 drawType;

        public AxisAlignedBounds2D localBounds;

        public MaterialId materialId;

        public float4x4* matrix;
        public void* shapeData;
        public void* materialData;
        public MaterialPropertyOverride* materialOverrideValues;
        public int materialOverrideCount;

        public bool IsNonRendering() {
            return (drawType == DrawType2.PushClipRect ||
                    drawType == DrawType2.PushClipScope ||
                    drawType == DrawType2.PushStencilClip ||
                    drawType == DrawType2.PopClipper ||
                    drawType == DrawType2.Callback);
        }

        public bool IsInternalDrawType() {
            return (drawType == DrawType2.UIForiaText ||
                    drawType == DrawType2.UIForiaElement ||
                    drawType == DrawType2.UIForiaGeometry ||
                    drawType == DrawType2.UIForiaSDF);
        }

    }

    public struct ElementMaterialSetup {

        public TextureUsage bodyTexture;
        public TextureUsage outlineTexture;
        public ElementMaterialInfo materialInfo;

    }

    public struct TextMaterialSetup {

        public TextureUsage faceTexture;
        public TextureUsage outlineTexture;
        public TextMaterialInfo materialInfo;
        public int fontTextureId;

    }

    [AssertSize(32)]
    [StructLayout(LayoutKind.Sequential)]
    public struct UIForiaVertex {

        // i could drop indices by splitting the actual index buffer values,
        // but that imposes limits on material and vertex count because id
        // split the 32 bit range into something like 22 & 10

        // i dont need a z value for position, its always 0
        // to support 3d id either bake the zinto the material data or the transform matrix
        public float2 position;
        public float2 texCoord0;
        public float2 texCoord1; // theres a small chance I don't need this channel at all
        public int2 indices;

        public UIForiaVertex(float x, float y) {
            position.x = x;
            position.y = y;
            texCoord0 = default;
            texCoord1 = default;
            indices = default;
        }

    }

    public unsafe struct SDFTextMeshDesc {

        public void* meshDataList;
        public bool requiresBaking;
        public int fontAssetId;
        public int symbolStart;
        public int symbolEnd;
        public FontStyle fontStyle;
        public float fontSize;

    }

    public struct SDFMeshDesc {

        public float x;
        public float y;
        public ElementMeshStyle meshStyle;
        public AxisAlignedBounds2D uvRect;
        public float width;
        public float height;

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

        private GraphicsBuffer indexBuffer;
        private ComputeBuffer indirectArgsBuffer;
        private ComputeBuffer vertexBuffer;
        private ComputeBuffer matrixBuffer;
        private ComputeBuffer clipBuffer;
        private ComputeBuffer materialBuffer;
        private ComputeBuffer glyphBuffer;
        private ComputeBuffer fontBuffer;
        private bool needsFontUpload;

        public RenderSystem2(Application application, LayoutSystem layoutSystem, ElementSystem elementSystem) {
            this.elementSystem = elementSystem;
            this.layoutSystem = layoutSystem;
            this.application = application;
            this.renderBoxPool = new RenderBoxPool(application.ResourceManager);
            this.resourceManager = application.ResourceManager;
            this.mpb = new MaterialPropertyBlock();
            this.renderContext = new RenderContext3(resourceManager);

            this.indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, 1024, sizeof(int));
            this.vertexBuffer = new ComputeBuffer(1024, sizeof(UIForiaVertex), ComputeBufferType.Structured);
            this.clipBuffer = new ComputeBuffer(16, sizeof(AxisAlignedBounds2D), ComputeBufferType.Structured); // could be cbuffer;
            this.matrixBuffer = new ComputeBuffer(256, sizeof(float4x4), ComputeBufferType.Structured); // could be cbuffer, its small enough
            this.materialBuffer = new ComputeBuffer(256, sizeof(UIForiaMaterialInfo), ComputeBufferType.Structured); // could be cbuffer, its small enough

            this.glyphBuffer = new ComputeBuffer(512, sizeof(GPUGlyphInfo), ComputeBufferType.Structured); // todo -- try as constant
            this.fontBuffer = new ComputeBuffer(8, sizeof(GPUFontInfo), ComputeBufferType.Structured); // todo -- try as constant
            this.indirectArgsBuffer = new ComputeBuffer(16, sizeof(IndirectArg), ComputeBufferType.IndirectArguments);

            materialBuffer.name = "UIForia::MaterialBuffer";
            matrixBuffer.name = "UIForia::MatrixBuffer";
            vertexBuffer.name = "UIForia::VertexBuffer";
            clipBuffer.name = "UIForia::ClipBuffer";
            unsafeData.Initialize(this);
            resourceManager.onFontAdded += HandleFontAdded;

            needsFontUpload = true;

        }

        private void HandleFontAdded(FontAsset fontAsset) {
            needsFontUpload = true;
        }

        public void SetCamera(Camera camera) { }

        public void Dispose() {

            indexBuffer.Dispose();
            vertexBuffer.Dispose();
            matrixBuffer.Dispose();
            glyphBuffer.Dispose();
            clipBuffer.Dispose();
            materialBuffer.Dispose();
            indirectArgsBuffer.Dispose();

            unsafeData.Dispose();
        }

        public virtual void OnUpdate() {

            unsafeData.Clear();
            renderContext.Clear();

            resourceManager.GetFontTextures(renderContext.textureMap);

            // todo -- if resourceManager

            List<UIView> views = layoutSystem.application.views;

            unsafeData.perViewElementList.SetSize(views.Count);

            for (int i = 0; i < views.Count; i++) {

                LayoutContext layoutContext = layoutSystem.GetLayoutContext(views[i]);

                unsafeData.perViewElementList[i] = new ElementList() {
                    size = layoutContext.elementList.size,
                    array = layoutContext.elementList.GetArrayPointer()
                };

            }

            UIForiaScheduler.Run(new GatherRenderedElements() {
                elementLists = unsafeData.perViewElementList,
                traversalTable = elementSystem.traversalTable,
                clipInfoTable = layoutSystem.clipInfoTable,
                renderCallList = unsafeData.renderCallList,
                renderInfoTable = unsafeData.renderInfoTable,
            });

            // todo -- if i end up using native painters, will need to include them in draw list
            RunMainThreadPainters();

            unsafeData.drawList.AddRange(renderContext.drawList.GetArrayPointer(), renderContext.drawList.size);

            new SortDrawInfoList() {drawList = unsafeData.drawList}.Run();

            unsafeData.transformedBounds.SetSize(unsafeData.drawList.size);
            unsafeData.materialPermutationIds.SetSize(unsafeData.drawList.size);
            unsafeData.matrixIdList.SetSize(unsafeData.drawList.size);
            unsafeData.materialIdList.SetSize(unsafeData.drawList.size);
            unsafeData.meshInfoList.SetSize(unsafeData.drawList.size);
            unsafeData.clipRectIdList.SetSize(unsafeData.drawList.size);
            unsafeData.renderTraversalList.SetSize(unsafeData.drawList.size);

            JobHandle processClipping = UIForiaScheduler.Run(new ProcessClipping() {
                drawList = unsafeData.drawList,
                transformedBounds = unsafeData.transformedBounds,
                // todo -- verify this should really be dpi scaled
                surfaceWidth = application.Width,
                surfaceHeight = application.Height,
                clipRectIdList = unsafeData.clipRectIdList,
                clipperBoundsList = unsafeData.clipRectBuffer,
                clipRenderList = unsafeData.renderTraversalList,
                stencilDataList = unsafeData.stencilDataList

                // todo -- more data needed

            });

            NativeArray<JobHandle> preProcessHandles = new NativeArray<JobHandle>(5, Allocator.Temp);
            NativeArray<JobHandle> shapeBakingHandles = new NativeArray<JobHandle>(3, Allocator.Temp);

            shapeBakingHandles[0] = UIForiaScheduler.Run(new BakeUIForiaElements() {
                drawList = unsafeData.drawList,
                triangleList = unsafeData.elementTriangleList,
                vertexList = unsafeData.elementVertexList,
                meshInfoList = unsafeData.meshInfoList
            });

            shapeBakingHandles[1] = UIForiaScheduler.Run(new BakeUIForiaText() {
                drawList = unsafeData.drawList,
                triangleList = unsafeData.textTriangleList,
                vertexList = unsafeData.textVertexList,
                meshInfoList = unsafeData.meshInfoList
            });

            shapeBakingHandles[2] = UIForiaScheduler.Run(new BakeUIForiaShapes() {
                drawList = unsafeData.drawList,
                triangleList = unsafeData.shapeTriangleList,
                vertexList = unsafeData.shapeVertexList,
                meshInfoList = unsafeData.meshInfoList
            });

            preProcessHandles[0] = UIForiaScheduler.Run(new BuildMaterialPermutations() {
                drawList = unsafeData.drawList,
                permutationList = unsafeData.materialPermutationsList,
                materialPermutationIdList = unsafeData.materialPermutationIds,
            });

            preProcessHandles[1] = UIForiaScheduler.Run(new BuildUVTransforms() {
                drawList = unsafeData.drawList
            });

            preProcessHandles[2] = UIForiaScheduler.Run(new BuildUIForiaMaterialBuffer() {
                drawList = unsafeData.drawList,
                materialBuffer = unsafeData.materialBuffer,
                materialIdList = unsafeData.materialIdList
            });

            preProcessHandles[4] = UIForiaScheduler.Run(new BuildMatrixBuffer() {
                drawList = unsafeData.drawList,
                matrixBuffer = unsafeData.matrixBuffer,
                matrixIndices = unsafeData.matrixIdList
            });

            JobHandle buildData = JobHandle.CombineDependencies(preProcessHandles);

            JobHandle ready = JobHandle.CombineDependencies(buildData, processClipping);

            JobHandle shapeBaking = JobHandle.CombineDependencies(shapeBakingHandles);

            // JobHandle uiforiaShapeBaking = UIForiaScheduler.Await(buildData, shapeBaking)
            //     .Then(new CombineUIForiaVertexBuffers() {
            //
            //         textVertexList = unsafeData.textVertexList,
            //         textTriangleList = unsafeData.textTriangleList,
            //
            //         elementVertexList = unsafeData.elementVertexList,
            //         elementTriangleList = unsafeData.elementTriangleList,
            //
            //         meshList = unsafeData.meshInfoList,
            //         materialIdList = unsafeData.materialIdList,
            //         matrixIdList = unsafeData.matrixIdList,
            //         clipRectIdList = unsafeData.clipRectIdList
            //
            //     });
            new CombineUIForiaVertexBuffers() {

                textVertexList = unsafeData.textVertexList,
                textTriangleList = unsafeData.textTriangleList,

                elementVertexList = unsafeData.elementVertexList,
                elementTriangleList = unsafeData.elementTriangleList,

                meshList = unsafeData.meshInfoList,
                materialIdList = unsafeData.materialIdList,
                matrixIdList = unsafeData.matrixIdList,
                clipRectIdList = unsafeData.clipRectIdList

            }.Run();

            // JobHandle renderPassHandle = UIForiaScheduler.Await(ready)
            //     .Then(new BuildRenderPasses() {
            //         drawList = unsafeData.drawList,
            //         batchList = unsafeData.batchList,
            //         batchMemberList = unsafeData.batchMemberList,
            //         materialPermutationIds = unsafeData.materialPermutationIds,
            //         materialPermutations = unsafeData.materialPermutationsList,
            //         renderCommands = unsafeData.renderCommands,
            //         stencilList = default,
            //         clipperBoundsList = unsafeData.clipRectBuffer,
            //         renderTraversalList = unsafeData.renderTraversalList
            //     });
            // .Then(new BuildDynamicBatchGeometry());
            new BuildRenderPasses() {
                drawList = unsafeData.drawList,
                batchList = unsafeData.batchList,
                batchMemberList = unsafeData.batchMemberList,
                materialPermutationIds = unsafeData.materialPermutationIds,
                materialPermutations = unsafeData.materialPermutationsList,
                renderCommands = unsafeData.renderCommands,
                stencilList = default,
                clipperBoundsList = unsafeData.clipRectBuffer,
                renderTraversalList = unsafeData.renderTraversalList
            }.Run();
            // renderHandle = JobHandle.CombineDependencies(uiforiaShapeBaking, renderPassHandle);
            new BuildIndexBuffer() {
                indirectArgs = unsafeData.indirectArgBuffer,
                meshInfoList = unsafeData.meshInfoList,
                batchList = unsafeData.batchList,
                batchMemberList = unsafeData.batchMemberList,
                indexBuffer = unsafeData.uiforiaIndexBuffer,
                combinedTriangleList = unsafeData.textTriangleList // use text list as the merged one, its naturally larger anyway
            }.Run();
            // renderHandle = UIForiaScheduler.Await(renderHandle).Then(new BuildIndexBuffer() {
            //     indirectArgs = unsafeData.indirectArgBuffer,
            //     meshInfoList = unsafeData.meshInfoList,
            //     batchList = unsafeData.batchList,
            //     batchMemberList = unsafeData.batchMemberList,
            //     indexBuffer = unsafeData.uiforiaIndexBuffer,
            //     combinedTriangleList = unsafeData.textTriangleList // use text list as the merged one, its naturally larger anyway
            // });

            // JobHandle.ScheduleBatchedJobs();

            // todo -- could be done later, will still need to upload meshes n stuff

            // foreach (KeyValuePair<int, Texture> kvp in renderContext.textureMap) {
            //     textureMap[kvp.Key] = kvp.Value;
            // }

            renderHandle.Complete();

            preProcessHandles.Dispose();
            shapeBakingHandles.Dispose();

        }

        private static readonly int s_VertexBufferKey = Shader.PropertyToID("_UIForiaVertexBuffer");
        private static readonly int s_MatrixBufferKey = Shader.PropertyToID("_UIForiaMatrixBuffer");
        private static readonly int s_ClipRectBufferKey = Shader.PropertyToID("_UIForiaClipRectBuffer");
        private static readonly int s_MaterialBufferKey = Shader.PropertyToID("_UIForiaMaterialBuffer");
        private static readonly int s_OriginMatrixKey = Shader.PropertyToID("_UIForiaOriginMatrix");
        private static readonly int s_FontDataBufferKey = Shader.PropertyToID("_UIForiaFontBuffer");
        private static readonly int s_GlyphBufferKey = Shader.PropertyToID("_UIForiaGlyphBuffer");
        private static readonly int s_FontTextureKey = Shader.PropertyToID("_FontTexture");

        private static readonly int s_MainTextureKey = Shader.PropertyToID("_MainTex");
        private static readonly int s_OutlineTextureKey = Shader.PropertyToID("_OutlineTex");

        public void Render(float surfaceWidth, float surfaceHeight, CommandBuffer commandBuffer) {
            // renderHandle.Complete();

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

            if (unsafeData.uiforiaIndexBuffer.size > indexBuffer.count) {
                indexBuffer.Dispose();
                indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, unsafeData.uiforiaIndexBuffer.capacity, sizeof(int));
            }

            if (unsafeData.materialBuffer.size > materialBuffer.count) {
                materialBuffer.Dispose();
                materialBuffer = new ComputeBuffer(unsafeData.materialBuffer.capacity, sizeof(UIForiaMaterialInfo), ComputeBufferType.Structured);
            }

            if (unsafeData.textVertexList.size > vertexBuffer.count) {
                vertexBuffer.Dispose();
                vertexBuffer = new ComputeBuffer(unsafeData.textVertexList.capacity, sizeof(UIForiaVertex), ComputeBufferType.Structured);
            }

            if (unsafeData.indirectArgBuffer.size > indirectArgsBuffer.count) {
                indirectArgsBuffer.Dispose();
                indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, unsafeData.indirectArgBuffer.capacity, sizeof(int));
            }

            // todo -- convert to float3x2 to save on upload cost in 2d case, and maybe a quat, vec3, vec3 for 3d?
            if (unsafeData.matrixBuffer.size > matrixBuffer.count) {
                matrixBuffer.Dispose();
                matrixBuffer = new ComputeBuffer(unsafeData.matrixBuffer.capacity, sizeof(float4x4), ComputeBufferType.Structured);
            }

            NativeArray<int> triangleArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(
                unsafeData.uiforiaIndexBuffer.GetArrayPointer(),
                unsafeData.uiforiaIndexBuffer.size,
                Allocator.None
            );

            NativeArray<UIForiaVertex> vertexArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<UIForiaVertex>(
                unsafeData.textVertexList.GetArrayPointer(),
                unsafeData.textVertexList.size,
                Allocator.None
            );

            NativeArray<IndirectArg> argArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<IndirectArg>(
                unsafeData.indirectArgBuffer.GetArrayPointer(),
                unsafeData.indirectArgBuffer.size,
                Allocator.None
            );

            NativeArray<float4x4> matrixArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float4x4>(
                unsafeData.matrixBuffer.GetArrayPointer(),
                unsafeData.matrixBuffer.size,
                Allocator.None
            );

            NativeArray<UIForiaMaterialInfo> materialArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<UIForiaMaterialInfo>(
                unsafeData.materialBuffer.GetArrayPointer(),
                unsafeData.materialBuffer.size,
                Allocator.None
            );

#if UNITY_EDITOR
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref triangleArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(unsafeData.dummyArray));
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref vertexArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(unsafeData.dummyArray));
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref matrixArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(unsafeData.dummyArray));
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref argArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(unsafeData.dummyArray));
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref materialArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(unsafeData.dummyArray));
#endif

            indexBuffer.SetData(triangleArray, 0, 0, unsafeData.uiforiaIndexBuffer.size);
            vertexBuffer.SetData(vertexArray, 0, 0, unsafeData.textVertexList.size);
            matrixBuffer.SetData(matrixArray, 0, 0, unsafeData.matrixBuffer.size);
            materialBuffer.SetData(materialArray, 0, 0, unsafeData.materialBuffer.size);
            indirectArgsBuffer.SetData(argArray, 0, 0, unsafeData.indirectArgBuffer.size);

            RenderCommand* renderCommands = unsafeData.renderCommands.GetArrayPointer();
            int renderCommandCount = unsafeData.renderCommands.size;

            float halfWidth = surfaceWidth * 0.5f;
            float halfHeight = surfaceHeight * 0.5f;

            Matrix4x4 origin = Matrix4x4.TRS(new Vector3(-halfWidth, halfHeight, 0), Quaternion.identity, Vector3.one);

            commandBuffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.Ortho(-halfWidth, halfWidth, -halfHeight, halfHeight, -100, 100));

            // todo -- need to double buffer these
            commandBuffer.SetGlobalBuffer(s_VertexBufferKey, vertexBuffer);
            commandBuffer.SetGlobalBuffer(s_MatrixBufferKey, matrixBuffer);
            commandBuffer.SetGlobalBuffer(s_ClipRectBufferKey, clipBuffer);
            commandBuffer.SetGlobalBuffer(s_MaterialBufferKey, materialBuffer);
            commandBuffer.SetGlobalBuffer(s_FontDataBufferKey, fontBuffer);
            commandBuffer.SetGlobalBuffer(s_GlyphBufferKey, glyphBuffer);

            commandBuffer.SetGlobalMatrix(s_OriginMatrixKey, origin);

            for (int i = 0; i < renderCommandCount; i++) {

                ref RenderCommand renderCommand = ref renderCommands[i];
                switch (renderCommand.type) {

                    case RenderCommandType.ElementBatch: {

                        mpb.Clear();

                        Batch batch = unsafeData.batchList[renderCommand.batchIndex];

                        Material material = resourceManager.GetMaterialInstance(MaterialId.UIForiaShape);

                        if (renderContext.textureMap.TryGetValue(batch.materialPermutation.texture0, out Texture texture)) {
                            mpb.SetTexture(s_MainTextureKey, texture);
                        }

                        if (renderContext.textureMap.TryGetValue(batch.materialPermutation.texture1, out texture)) {
                            mpb.SetTexture(s_OutlineTextureKey, texture);
                        }

                        commandBuffer.DrawProceduralIndirect(indexBuffer, Matrix4x4.identity, material, 0, MeshTopology.Triangles, indirectArgsBuffer, sizeof(IndirectArg) * batch.indirectArgOffset, mpb);

                        break;
                    }

                    case RenderCommandType.ShapeEffectBatch:
                        break;

                    case RenderCommandType.Mesh:
                        break;

                    case RenderCommandType.MeshBatch:
                        break;

                    case RenderCommandType.SDFTextBatch: {
                        mpb.Clear();

                        Batch batch = unsafeData.batchList[renderCommand.batchIndex];

                        Material material = resourceManager.GetMaterialInstance(MaterialId.UIForiaSDFText);

                        if (renderContext.textureMap.TryGetValue(batch.materialPermutation.texture0, out Texture texture)) {
                            mpb.SetTexture(s_MainTextureKey, texture);
                        }

                        if (renderContext.textureMap.TryGetValue(batch.materialPermutation.texture1, out texture)) {
                            mpb.SetTexture(s_OutlineTextureKey, texture);
                        }

                        if (renderContext.textureMap.TryGetValue(batch.materialPermutation.texture2, out texture)) {
                            mpb.SetTexture(s_FontTextureKey, texture);
                        }

                        commandBuffer.DrawProceduralIndirect(indexBuffer, Matrix4x4.identity, material, 0, MeshTopology.Triangles, indirectArgsBuffer, sizeof(IndirectArg) * batch.indirectArgOffset, mpb);

                        break;
                    }

                    case RenderCommandType.SDFTextEffectBatch:
                        break;

                    case RenderCommandType.CreateRenderTarget:
                        break;

                    case RenderCommandType.PushRenderTexture:
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

                RenderBox box = renderBoxTable[elementId.index];

                if (box == null) continue;

                MaterialId materialId = box.materialId;

                ref LayoutBoxInfo layoutResult = ref layoutSystem.layoutResultTable.array[elementId.index];

                if (layoutResult.sizeChanged) {
                    layoutResult.sizeChanged = false;
                    box.OnSizeChanged(new Size(layoutResult.actualSize.x, layoutResult.actualSize.y));
                }

                if (callInfo.renderOp == 0) {
                    // todo -- if clipper via styles, set that up here
                    renderContext.Setup(materialId, i, matrices + elementId.index);
                    // todo -- if is clipper, push clip rect or stencil, probably after the draw
                    box.PaintBackground3(renderContext);
                }
                else {
                    // todo -- if clipper via styles, tear it down here
                    renderContext.Setup(materialId, i, matrices + elementId.index);
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

                string painter = instance.style.Painter;

                RenderBox renderBox = null;

                if (painter != null) {
                    renderBox = renderBoxPool.GetCustomPainter(painter);
                }

                if (renderBox == null) {
                    if (instance is UITextElement) {
                        renderBox = new TextRenderBox2();
                    }
                    else {
                        renderBox = new StandardRenderBox2();
                    }
                }

                renderBoxTable[elementId.index] = renderBox;

                unsafeData.renderInfoTable[elementId] = new RenderInfo() {
                    layer = instance.style.Layer,
                    zIndex = instance.style.ZIndex,
                    drawForeground = renderBox.HasForeground,
                    hasBackgroundEffect = false,
                    hasForegroundEffect = false
                };

                instance.renderBox = renderBox; // todo -- remove link
                renderBox.element = instance;
                instance.renderBox.OnInitialize();

            }

        }

        public void HandleElementsDisabled(DataList<ElementId>.Shared disabledElements) { }

        public void HandleStylePropertyUpdates(UIElement element, StyleProperty[] propertyList, int propertyCount) {
            //   if (element.renderBox == null) return;

            // for (int i = 0; i < propertyCount; i++) {
            //     ref StyleProperty property = ref propertyList[i];
            //     switch (property.propertyId) {
            //         case StylePropertyId.Painter:
            //             ReplaceRenderBox(element, property.AsString);
            //             break;
            //     }
            // }

            //element.renderBox.OnStylePropertyChanged(propertyList, propertyCount);
        }

        private struct UnsafeData : IDisposable {

            public DataList<DrawInfo2>.Shared drawList;
            public DataList<ElementList> perViewElementList;
            public DataList<RenderCallInfo>.Shared renderCallList;
            public ElementTable<RenderInfo> renderInfoTable;
            public DataList<AxisAlignedBounds2D> transformedBounds;
            public DataList<int>.Shared uiforiaIndexBuffer;

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
            public DataList<int>.Shared elementTriangleList;
            public DataList<UIForiaVertex>.Shared elementVertexList;
            public DataList<MeshInfo> meshInfoList;
            public DataList<int>.Shared textTriangleList;
            public DataList<UIForiaVertex>.Shared textVertexList;
            public DataList<UIForiaVertex>.Shared shapeVertexList;
            public DataList<int>.Shared shapeTriangleList;
            public DataList<UIForiaMaterialInfo>.Shared materialBuffer;

            public DataList<IndirectArg>.Shared indirectArgBuffer;
            public NativeArray<int> dummyArray;
            public DataList<GPUGlyphInfo> glyphBuffer;
            public DataList<GPUFontInfo> fontBuffer;

            public void Initialize(RenderSystem2 renderSystem2) {
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

                this.uiforiaIndexBuffer = new DataList<int>.Shared(1024, Allocator.Persistent);

                this.matrixIdList = new List_Int32(64, Allocator.Persistent);
                this.materialPermutationIds = new List_Int32(64, Allocator.Persistent);
                this.clipRectIdList = new List_Int32(64, Allocator.Persistent);
                this.batchList = new DataList<Batch>.Shared(16, Allocator.Persistent);
                this.batchMemberList = new DataList<int>.Shared(64, Allocator.Persistent);

                this.materialIdList = new List_Int32(64, Allocator.Persistent);
                this.materialBuffer = new DataList<UIForiaMaterialInfo>.Shared(64, Allocator.Persistent);

                this.elementTriangleList = new DataList<int>.Shared(6 * 64, Allocator.Persistent);
                this.elementVertexList = new DataList<UIForiaVertex>.Shared(4 * 64, Allocator.Persistent);

                this.textTriangleList = new DataList<int>.Shared(6 * 64, Allocator.Persistent);
                this.textVertexList = new DataList<UIForiaVertex>.Shared(4 * 64, Allocator.Persistent);

                this.shapeVertexList = new DataList<UIForiaVertex>.Shared(16, Allocator.Persistent);
                this.shapeTriangleList = new DataList<int>.Shared(16, Allocator.Persistent);
                this.meshInfoList = new DataList<MeshInfo>(64, Allocator.Persistent);

            }

            public void Clear() {
                Profiler.BeginSample("Clear Render Setup");
                renderCommands.size = 0;

                indirectArgBuffer.size = 0;
                uiforiaIndexBuffer.size = 0;

                materialBuffer.size = 0;
                shapeTriangleList.size = 0;
                shapeVertexList.size = 0;
                materialIdList.size = 0;
                textTriangleList.size = 0;
                textVertexList.size = 0;
                meshInfoList.size = 0;
                elementTriangleList.size = 0;
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
                glyphBuffer.Dispose();
                meshInfoList.Dispose();
                dummyArray.Dispose();
                indirectArgBuffer.Dispose();
                materialIdList.Dispose();

                textVertexList.Dispose();
                textTriangleList.Dispose();
                elementTriangleList.Dispose();
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