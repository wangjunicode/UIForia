using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using ThisOtherThing.UI;
using ThisOtherThing.UI.ShapeUtils;
using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Graphics.ShapeKit;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Batch = UIForia.Graphics.Batch;

namespace UIForia.Systems {

    public unsafe class RenderSystem : IRenderSystem, IDisposable {

        public event Action<RenderContext> DrawDebugOverlay;

        private ElementSystem elementSystem;
        private LayoutSystem layoutSystem;
        private RenderBoxPool renderBoxPool;
        private ElementTable<RenderInfo> renderInfoTable;
        private RenderBox[] renderBoxTable;
        private DataList<ElementId> renderedElementIds;
        private DataList<RenderCallInfo>.Shared renderCallList;
        private ResourceManager resourceManager;
        private int elementCapacity;
        private LightList<Mesh> meshList;
        private LightList<Mesh> batchMeshList;
        private LightList<Mesh> maskMeshList;
        private JobHandle renderHandle;
        private MaterialPropertyBlock mpb;
        private ThreadSafePool<RenderContext2> renderContextPool;

        private PersistentData persistentData;
        private Dictionary<int, Texture> textureMap;
        private LightList<RenderTexture> maskTextures;
        private CommandBuffer maskCommandBuffer;

        public RenderSystem(Application application, LayoutSystem layoutSystem, ElementSystem elementSystem) {
            this.elementSystem = elementSystem;
            this.renderBoxPool = new RenderBoxPool(application.ResourceManager);
            this.resourceManager = application.ResourceManager;
            this.layoutSystem = layoutSystem;
            this.meshList = new LightList<Mesh>();
            this.renderedElementIds = new DataList<ElementId>(128, Allocator.Persistent);
            this.renderCallList = new DataList<RenderCallInfo>.Shared(64, Allocator.Persistent);
            this.textureMap = new Dictionary<int, Texture>();
            this.batchMeshList = new LightList<Mesh>(32);
            this.maskMeshList = new LightList<Mesh>();
            this.maskCommandBuffer = new CommandBuffer();
            this.maskCommandBuffer.name = "UIForia Mask Generator";
            this.maskTextures = new LightList<RenderTexture>();
            this.renderContextPool = new ThreadSafePool<RenderContext2>(
                JobsUtility.MaxJobThreadCount,
                () => new RenderContext2(this, application.ResourceManager),
                (ctx) => ctx.Clear()
            );

            this.mpb = new MaterialPropertyBlock();

            InitPersistentData();
            // application.onViewsSorted += uiViews => { renderOwners.Sort((o1, o2) => o1.view.Depth.CompareTo(o2.view.Depth)); };
        }

        private void ResizeBackingStore(int newCapacity) {

            persistentData.renderBoxTableHandle.Dispose();

            Array.Resize(ref renderBoxTable, newCapacity * 2);

            persistentData.renderBoxTableHandle = new GCHandleArray<RenderBox>(renderBoxTable);

            RenderInfo* ptr = TypedUnsafe.Malloc<RenderInfo>(newCapacity * 2, Allocator.Persistent);
            if (renderInfoTable.array != null) {
                TypedUnsafe.MemCpy(ptr, renderInfoTable.array, elementCapacity);
                TypedUnsafe.Dispose(renderInfoTable.array, Allocator.Persistent);
            }

            renderInfoTable.array = ptr;
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

                renderInfoTable[elementId] = new RenderInfo() {
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

        private void InitPersistentData() {

            persistentData = new PersistentData() {
                clipperBoundsList = new DataList<AxisAlignedBounds2D>.Shared(32, Allocator.Persistent),
                stencilList = new DataList<StencilInfo>.Shared(32, Allocator.Persistent),
                maskElementList = new DataList<MaskInfo>.Shared(16, Allocator.Persistent),
                maskTargetList = new DataList<MaskTarget>.Shared(16, Allocator.Persistent),
                maskGeometryAllocator = new PagedByteAllocator(TypedUnsafe.Kilobytes(16), Allocator.Persistent, Allocator.TempJob),
                meshListHandle = new GCHandleList<Mesh>(meshList),
                drawList = new DataList<DrawInfo>.Shared(32, Allocator.Persistent),
                effectUsageList = new DataList<EffectUsage>.Shared(8, Allocator.Persistent),
                perViewElementList = new DataList<ElementList>(4, Allocator.Persistent),
                contextPoolHandle = new PerThreadObjectPool<RenderContext2>(renderContextPool),
                renderBoxTableHandle = new GCHandleArray<RenderBox>(renderBoxTable),
                renderContextInfoHandle = new GCHandle<RenderSystem>(this),
                batchList = new DataList<Batch>.Shared(16, Allocator.Persistent),
                batchMemberList = new DataList<int>.Shared(32, Allocator.Persistent),
                shapeBuffer = new PerThread<ShapeDataBuffer>(Allocator.Persistent),
                renderCommands = new DataList<RenderCommand>.Shared(64, Allocator.Persistent),
                perThreadGeometryBuffer = new PerThread<GeometryBuffer>(Allocator.Persistent),
                dummyArray = new NativeArray<int>(1, Allocator.Persistent), // this used to get an Atomic Safety handle for meshes
                boundsAllocator = new PagedByteAllocator(TypedUnsafe.Kilobytes(1), Allocator.Persistent, Allocator.TempJob),

                textureMapHandle = new GCHandle<Dictionary<int, Texture>>(textureMap)
            };
        }

        public virtual void OnUpdate() {

            // todo -- hook this up to the layout system's job scheduling
            renderContextPool.Clear();
            persistentData.Clear();
            textureMap.Clear();
            resourceManager.GetFontTextures(textureMap);
            List<UIView> views = layoutSystem.application.views;

            persistentData.perViewElementList.SetSize(views.Count);

            for (int i = 0; i < views.Count; i++) {

                LayoutContext layoutContext = layoutSystem.GetLayoutContext(views[i]);

                persistentData.perViewElementList[i] = new ElementList() {
                    size = layoutContext.elementList.size,
                    array = layoutContext.elementList.GetArrayPointer()
                };

            }

            JobHandle gatherRenderedElements = VertigoScheduler.Run(new GatherRenderedElements() {
                elementLists = persistentData.perViewElementList,
                effectUsages = persistentData.effectUsageList,
                traversalTable = elementSystem.traversalTable,
                clipInfoTable = layoutSystem.clipInfoTable,
                renderCallList = renderCallList,
                renderInfoTable = renderInfoTable,
            });

            // assumes we are able to call all painters in parallel, this miiiight backfire
            JobHandle invokePainters = VertigoScheduler.Await(gatherRenderedElements).ThenParallel(new CallPainters_Managed() {
                    parallel = new ParallelParams(renderCallList.size, 32),
                    renderCallList = renderCallList,
                    matrices = layoutSystem.worldMatrices,
                    renderContextPool = persistentData.contextPoolHandle,
                    renderBoxTableHandle = persistentData.renderBoxTableHandle,
                    clipInfoTable = layoutSystem.clipInfoTable,
                    clipperBoundsList = layoutSystem.clipperBoundsList
                })
                .Then(new MergeRenderContexts_Managed() {
                    maskList = persistentData.maskElementList,
                    contextPoolHandle = persistentData.contextPoolHandle,
                    drawList = persistentData.drawList,
                    meshListHandle = persistentData.meshListHandle,
                });

            invokePainters.Complete();

            JobHandle shapeBaking = VertigoScheduler.Await(invokePainters)
                .ThenParallel(new BakeShapes() {
                    parallel = new ParallelParams(persistentData.drawList.size, 10),
                    fontAssetMap = resourceManager.fontAssetMap,
                    drawList = persistentData.drawList,
                    perThread_ShapeBuffer = persistentData.shapeBuffer
                });

            JobHandle applyEffects = VertigoScheduler.Await(shapeBaking)
                .Then(new ApplyEffects_Managed() {
                    // I'm not sure if this can be parallel yet, I think so
                    effectUsageList = persistentData.effectUsageList,
                    renderBoxTableHandle = persistentData.renderBoxTableHandle
                });

            JobHandle gatherTextures = VertigoScheduler.Await(applyEffects).Then(new GatherTextures_Managed() {
                contextPoolHandle = persistentData.contextPoolHandle,
                textureMapHandle = persistentData.textureMapHandle
            });

            // JobHandle bakeClipping = VertigoScheduler.Await(applyEffects).Then(new BuildClipData() {
            //     drawList = persistentData.drawList,
            //     screenWidth = layoutSystem.application.Width,
            //     screenHeight = layoutSystem.application.Height
            // });
            //
            // JobHandle buildMaskAtlas = VertigoScheduler.Await(bakeClipping).Then(new BuildMaskElementAtlasJob() {
            //     maskElementList = persistentData.maskElementList,
            //     maskTargetList = persistentData.maskTargetList,
            //     batchList = persistentData.batchList,
            //     pagedByteAllocator = persistentData.maskGeometryAllocator
            // });

            JobHandle assignClippers = VertigoScheduler.Await(applyEffects)
                .Then(new AssignClippers() {
                    drawList = persistentData.drawList,
                    stencilList = persistentData.stencilList,
                    surfaceWidth = layoutSystem.application.Width,
                    surfaceHeight = layoutSystem.application.Height,
                    clipperBoundsList = persistentData.clipperBoundsList
                });

            JobHandle transparentPass = VertigoScheduler.Await(applyEffects)
                .Then(new TransparentRenderPassJob() {
                    drawList = persistentData.drawList,
                    batchList = persistentData.batchList,
                    batchMemberList = persistentData.batchMemberList,
                    renderCommands = persistentData.renderCommands,
                    boundsAllocator = persistentData.boundsAllocator,
                });

            transparentPass.Complete();

            // JobHandle buildGeometry = VertigoScheduler.Await( transparentPass, shapeBaking).Then(new BuildBatchGeometryJob() {
            //     drawList = persistentData.drawList,
            //     batchList = persistentData.batchList,
            //     batchMemberList = persistentData.batchMemberList,
            //     perThread_GeometryBuffer = persistentData.perThreadGeometryBuffer
            // });

            new BuildBatchGeometryJob() {
                drawList = persistentData.drawList,
                batchList = persistentData.batchList,
                batchMemberList = persistentData.batchMemberList,
                perThread_GeometryBuffer = persistentData.perThreadGeometryBuffer
            }.Run();

            renderHandle = JobHandle.CombineDependencies(gatherTextures, transparentPass);
            renderHandle.Complete();
            PopulateBatchMeshes();
            renderHandle = default;

        }

        // this could be parallel except mesh api won't allow it :(
        private void PopulateBatchMeshes() {
            Profiler.BeginSample("Populate Batch Meshes");
            DataList<Batch>.Shared batchList = persistentData.batchList;

            for (int batchIndex = 0; batchIndex < batchList.size; batchIndex++) {
                ref Batch batch = ref batchList[batchIndex];

                if (ReferenceEquals(batchMeshList.array[batchIndex], null)) {
                    batchMeshList.array[batchIndex] = new Mesh();
                    batchMeshList.array[batchIndex].MarkDynamic();
                    batchMeshList.array[batchIndex].indexFormat = IndexFormat.UInt32;
                }

                Mesh mesh = batchMeshList[batchIndex];
                // mesh.Clear();

                // mesh.SetVertexBufferParams(batch.vertexCount, layout);
                //
                // NativeArray<byte> vertArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(batch.geometryInterleaved, 48 * batch.vertexCount, Allocator.None);
                //
                // NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref vertArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(persistentData.dummyArray));
                //
                // mesh.SetVertexBufferData(vertArray, 0, 0, batch.vertexCount * 48,  0, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds);
                // mesh.SetIndexBufferParams(batch.triangleCount, IndexFormat.UInt32);
                // NativeArray<int> triangleArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(batch.triangles, batch.triangleCount, Allocator.None);
                // NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref triangleArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(persistentData.dummyArray));
                // mesh.SetIndexBufferData(triangleArray, 0, 0, triangleArray.Length);
                //
                // mesh.SetSubMesh(0, new SubMeshDescriptor(0, triangleArray.Length, MeshTopology.Triangles));

                for (int i = 0; i < batch.vertexChannelCount; i++) {

                    VertexChannelDesc channelDesc = *(batch.geometry + i);

                    switch (channelDesc.format) {

                        case VertexChannelFormat.Float1: {
                            NativeArray<Color32> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Color32>(channelDesc.ptr, batch.vertexCount, Allocator.None);
                            SetMeshChannel(mesh, array);
                            break;
                        }

                        case VertexChannelFormat.Float2: {
                            NativeArray<float2> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float2>(channelDesc.ptr, batch.vertexCount, Allocator.None);
                            SetMeshChannel(mesh, channelDesc, array);
                            break;
                        }

                        case VertexChannelFormat.Float3: {
                            NativeArray<float3> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float3>(channelDesc.ptr, batch.vertexCount, Allocator.None);
                            SetMeshChannel(mesh, channelDesc, array);
                            break;
                        }

                        case VertexChannelFormat.Float4: {
                            NativeArray<float4> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float4>(channelDesc.ptr, batch.vertexCount, Allocator.None);
                            SetMeshChannel(mesh, channelDesc, array);
                            break;
                        }

                    }

                }

                NativeArray<int> triangleArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(batch.triangles, batch.triangleCount, Allocator.None);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref triangleArray, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(persistentData.dummyArray));
                mesh.SetIndices(triangleArray, 0, triangleArray.Length, MeshTopology.Triangles, 0, false);

            }

            // todo -- need to consider how we destroy the meshes, probably just run through on destruction and call destroy if not null regardless of size
            Profiler.EndSample();
        }

        // todo -- get rid of camera, just don't know how to compute the matrices by hand
        public void Render(float surfaceWidth, float surfaceHeight, CommandBuffer commandBuffer) {

            float halfWidth = surfaceWidth * 0.5f;
            float halfHeight = surfaceHeight * 0.5f;
            DataList<RenderCommand>.Shared renderCommands = persistentData.renderCommands;
            commandBuffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.Ortho(-halfWidth, halfWidth, -halfHeight, halfHeight, -100, 100));

            int batchListIdx = 0;

            Matrix4x4 origin = Matrix4x4.TRS(new Vector3(-halfWidth, halfHeight, 0), Quaternion.identity, Vector3.one);

            for (int i = 0; i < renderCommands.size; i++) {

                ref RenderCommand cmd = ref renderCommands[i];
                switch (cmd.type) {
                    case RenderCommandType.ShapeBatch:
                        Mesh mesh = batchMeshList[batchListIdx++];
                        int batchIndex = cmd.batchIndex;
                        ref Batch batch = ref persistentData.batchList[batchIndex];

                        // todo -- when i implement addressables, this should be done as close to the start of frame as possible 
                        Material material = resourceManager.GetMaterialInstance(batch.materialId);

                        Matrix4x4 matrix;

                        bool isUIForiaMaterial = batch.HasUIForiaMaterial();

                        if (batch.memberIdRange.length == 1) {
                            matrix = *persistentData.drawList[persistentData.batchMemberList[batch.memberIdRange.start]].matrix;
                            matrix *= origin;
                        }
                        else {
                            matrix = origin;
                        }

                        if (batch.propertyOverrideCount > 0 || isUIForiaMaterial) {
                            SetupMaterialPropertyBlock(batch, isUIForiaMaterial);
                            commandBuffer.DrawMesh(mesh, matrix, material, 0, 0, mpb);
                        }
                        else {
                            commandBuffer.DrawMesh(mesh, matrix, material);
                        }

                        break;

                    // case CommandType.DrawMesh: {
                    //     ref Batch batch = ref inputBatchList[cmd.batchIndex];
                    //
                    //     Mesh mesh = userMeshList[batch.meshId];
                    //     Material material = materialList[batch.materialKey.materialIndex];
                    //
                    //     float4x4 matrix = batch.transformIndex == 0
                    //         ? layoutSystem.worldMatrices.array[batch.transformIndex]
                    //         : default; //renderContext.transformList[batch.transformIndex];
                    //
                    //     if (batch.materialKey.propertyCount > 0) {
                    //         mpb.Clear();
                    //         commandBuffer.DrawMesh(mesh, matrix, material, 0, 0, mpb);
                    //     }
                    //     else {
                    //         commandBuffer.DrawMesh(mesh, matrix, material, 0, 0);
                    //     }
                    //
                    //     break;
                    // }
                    //
                    // case CommandType.DrawBatch: {
                    //     ref Batch batch = ref inputBatchList[cmd.batchIndex];
                    //     Mesh mesh = batchMeshList[batch.meshId];
                    //     Material material = materialList[batch.materialKey.materialIndex];
                    //     if (batch.materialKey.propertyCount > 0) {
                    //         mpb.Clear();
                    //         commandBuffer.DrawMesh(mesh, Matrix4x4.identity, material, 0, 0, mpb);
                    //     }
                    //     else {
                    //         commandBuffer.DrawMesh(mesh, Matrix4x4.identity, material, 0, 0);
                    //     }
                    //
                    //     break;
                    // }

                }
            }

        }

        private static readonly List<Vector4> s_OverflowClippers = new List<Vector4>(3 * 4);
        private static readonly int s_ClippersKey = Shader.PropertyToID("_OverflowClippers");

        private void SetupMaterialPropertyBlock(in Batch batch, bool isUIForiaMaterial) {
            mpb.Clear();

            if (isUIForiaMaterial) {

                s_OverflowClippers.Clear();
                OverflowBounds bounds = new OverflowBounds() {
                    p0 = new float2(0, 0),
                    p1 = new float2(2000, 0),
                    p2 = new float2(2000, 2000),
                    p3 = new float2(0, 2000),
                };
                s_OverflowClippers.Add(new Vector4(bounds.p0.x, bounds.p0.y, bounds.p1.x, bounds.p1.y));
                s_OverflowClippers.Add(new Vector4(bounds.p2.x, bounds.p2.y, bounds.p3.x, bounds.p3.y));
                // for (int i = 0; i < batch.clipperCount; i++) {
                //     ref OverflowBounds bounds = ref batch.clippers[i];
                //     s_OverflowClippers.Add(new Vector4(bounds.p0.x, bounds.p0.y, bounds.p1.x, bounds.p1.y));
                //     s_OverflowClippers.Add(new Vector4(bounds.p2.x, bounds.p2.y, bounds.p3.x, bounds.p3.y));
                //     s_OverflowClippers.Add(new Vector4(bounds.p4.x, bounds.p4.y, bounds.p5.x, bounds.p5.y));
                // }

                mpb.SetVectorArray(s_ClippersKey, s_OverflowClippers);
            }

            for (int i = 0; i < batch.propertyOverrideCount; i++) {
                ref MaterialPropertyOverride property = ref batch.propertyOverrides[i];

                switch (property.propertyType) {

                    case MaterialPropertyType.Color:
                        mpb.SetColor(property.shaderPropertyId, property.value.colorValue);
                        break;

                    case MaterialPropertyType.Float:
                        mpb.SetFloat(property.shaderPropertyId, property.value.floatValue);
                        break;

                    case MaterialPropertyType.Vector:
                        mpb.SetVector(property.shaderPropertyId, property.value.vectorValue);
                        break;

                    case MaterialPropertyType.Texture:
                        if (textureMap.TryGetValue(property.value.textureId, out Texture texture)) {
                            mpb.SetTexture(property.shaderPropertyId, texture);
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
        }

        public void OnViewAdded(UIView view) {
            if (elementCapacity < view.dummyRoot.id.index) {
                if (elementCapacity == 0) {
                    ResizeBackingStore(32);
                }
                else {
                    ResizeBackingStore(view.dummyRoot.id.index * 2);
                }
            }

            renderInfoTable[view.dummyRoot.id] = default;
            // renderOwners.Add(new RenderOwner(view, elementSystem));
        }

        public void OnViewRemoved(UIView view) { }

        public void SetCamera(Camera camera) {
            // if (this.camera != null) {
            //     this.camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            // }
            //
            // this.camera = camera; // todo -- should be handled by the view
            //
            // if (this.camera != null) {
            //     this.camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            // }
        }

        public void Dispose() {
            renderHandle.Complete();

            renderedElementIds.Dispose();

            renderCallList.Dispose();

            // clear releases contexts to queue, need this to dispose them
            renderContextPool.Clear();

            foreach (RenderContext2 renderContext2 in renderContextPool.queue) {
                renderContext2.Dispose();
            }

            TypedUnsafe.Dispose(renderInfoTable.array, Allocator.Persistent);
            persistentData.Dispose();
            for (int i = 0; i < maskTextures.size; i++) {
                maskTextures[i].Release();
            }
        }

        private void SetMeshChannel(Mesh mesh, VertexChannelDesc desc, NativeArray<float4> array) {
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(persistentData.dummyArray));

            switch (desc.channel) {

                case VertexChannel.Color:
                    mesh.SetColors(array, 0, array.Length);
                    break;

                case VertexChannel.Tangent:
                    mesh.SetTangents(array, 0, array.Length);
                    break;

                case VertexChannel.TextureCoord0:
                case VertexChannel.TextureCoord1:
                case VertexChannel.TextureCoord2:
                case VertexChannel.TextureCoord3:
                case VertexChannel.TextureCoord4:
                case VertexChannel.TextureCoord5:
                case VertexChannel.TextureCoord6:
                case VertexChannel.TextureCoord7:
                    int channel = desc.GetUVChannelIndex();
                    if (channel < 0 || channel > 7) {
                        return;
                    }

                    mesh.SetUVs(channel, array, 0, array.Length);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetMeshChannel(Mesh mesh, in VertexChannelDesc desc, NativeArray<float3> array) {
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(persistentData.dummyArray));
            switch (desc.channel) {

                case VertexChannel.Position:
                    mesh.SetVertices(array, 0, array.Length);
                    break;

                case VertexChannel.Normal:
                    mesh.SetNormals(array, 0, array.Length);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetMeshChannel(Mesh mesh, NativeArray<Color32> array) {
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(persistentData.dummyArray));
            mesh.SetColors(array, 0, array.Length);
        }

        private void SetMeshChannel(Mesh mesh, VertexChannelDesc desc, NativeArray<float2> array) {
            int channel = desc.GetUVChannelIndex();
            if (channel < 0 || channel > 7) {
                return;
            }

            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, NativeArrayUnsafeUtility.GetAtomicSafetyHandle(persistentData.dummyArray));
            mesh.SetUVs(channel, array, 0, array.Length);
        }

        private struct PersistentData : IDisposable {

            public DataList<EffectUsage>.Shared effectUsageList;
            public DataList<ElementList> perViewElementList;

            public PerThreadObjectPool<RenderContext2> contextPoolHandle;

            public GCHandleArray<RenderBox> renderBoxTableHandle;
            public GCHandle<RenderSystem> renderContextInfoHandle;
            public GCHandle<Dictionary<int, Texture>> textureMapHandle;

            public PerThread<ShapeDataBuffer> shapeBuffer;
            public DataList<Batch>.Shared batchList;
            public DataList<int>.Shared batchMemberList;
            public DataList<RenderCommand>.Shared renderCommands;
            public PerThread<GeometryBuffer> perThreadGeometryBuffer;
            public NativeArray<int> dummyArray;

            public PagedByteAllocator boundsAllocator;
            public PagedByteAllocator maskGeometryAllocator;

            public DataList<DrawInfo>.Shared drawList;
            public GCHandleList<Mesh> meshListHandle;

            public DataList<MaskInfo>.Shared maskElementList;
            public DataList<MaskTarget>.Shared maskTargetList;
            public DataList<StencilInfo>.Shared stencilList;
            public DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;

            public void Dispose() {
                stencilList.Dispose();
                clipperBoundsList.Dispose();
                maskElementList.Dispose();
                maskTargetList.Dispose();
                drawList.Dispose();
                meshListHandle.Dispose();
                maskGeometryAllocator.Dispose();
                effectUsageList.Dispose();
                boundsAllocator.Dispose();
                batchList.Dispose();
                textureMapHandle.Dispose();
                perThreadGeometryBuffer.Dispose();
                batchMemberList.Dispose();
                shapeBuffer.Dispose();
                perViewElementList.Dispose();
                contextPoolHandle.Dispose();
                renderBoxTableHandle.Dispose();
                renderContextInfoHandle.Dispose();
                renderCommands.Dispose();
                this = default;
            }

            public void Clear() {
                stencilList.size = 0;
                clipperBoundsList.size = 0;
                drawList.size = 0;
                maskElementList.size = 0;
                maskTargetList.size = 0;
                effectUsageList.size = 0;
                renderCommands.size = 0;
                batchMemberList.size = 0;
                batchList.size = 0;
                perViewElementList.size = 0;

                shapeBuffer.Clear();
                boundsAllocator.Clear();
                maskGeometryAllocator.Clear();
                perThreadGeometryBuffer.Clear();
            }

        }

        internal bool TryGetCachedDrawInfo(in ShapeCacheId shapeCacheId, out DrawInfo* drawInfo, out int count) {
            drawInfo = null;
            count = 0;
            return false;
        }

    }

    public unsafe struct ElementList {

        public int size;
        public ElementId* array;

    }

    public struct RenderCallInfo {

        public ElementId elementId;
        public int layer;
        public int ftbIndex;
        public int zIndex;
        public int renderOp;

    }

}