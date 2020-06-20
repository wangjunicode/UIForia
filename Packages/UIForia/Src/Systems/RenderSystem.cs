using System;
using System.Collections.Generic;
using ThisOtherThing.UI;
using ThisOtherThing.UI.ShapeUtils;
using UIForia.Elements;
using UIForia.Graphics;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Batch = UIForia.Graphics.Batch;

namespace UIForia.Systems {

    public struct RenderInfo {

        public int renderBoxId;
        public bool drawForeground;
        public int zIndex;
        public int layer;

    }

    // gather render commands into buffer
    // run them all together in burst land, generate meshes etc

    public enum DrawMode {

        Opaque,
        Transparent,
        Shadow

    }

    // somehow need to handle batching and clipping
    // if uiforia shader or material has property 'uiforia compatible', we encode the uiforia data into attributes
    // otherwise things like opacity, clip, mask uvs, etc, need to come come in from elsewhere

    public enum VertexComponent {

        X,
        Y,
        Z,
        W

    }

    public enum VertexChannelFormat {

        Off = 0,
        Float1 = 4,
        Float2 = 8,
        Float3 = 12,
        Float4 = 16,

    }

    [Flags]
    public enum VertexChannel {

        Position = 0,
        Normal = 1 << 0,
        Color = 1 << 1,
        Tangent = 1 << 2,
        TextureCoord0 = 1 << 3,
        TextureCoord1 = 1 << 4,
        TextureCoord2 = 1 << 5,
        TextureCoord3 = 1 << 6,
        TextureCoord4 = 1 << 7,
        TextureCoord5 = 1 << 8,
        TextureCoord6 = 1 << 9,
        TextureCoord7 = 1 << 10,

    }

    public struct RectData {

        public ShapeMode type;
        public float x;
        public float y;
        public float width;
        public float height;
        public Color32 color;
        public EdgeGradientData edgeGradient;

    }
    
    public struct RoundedRectData {

        public ShapeMode type;
        public float x;
        public float y;
        public float width;
        public float height;
        public Color32 color;
        public CornerProperties cornerProperties;
        public EdgeGradientData edgeGradient;

    }
    
    public unsafe class RenderSystem : IRenderSystem, IDisposable {

        private ElementSystem elementSystem;
        private LayoutSystem layoutSystem;
        private RenderBoxPool renderBoxPool;
        private ElementTable<RenderInfo> renderInfoTable;
        private RenderBox[] renderBoxTable;
        private DataList<ElementId> renderedElementIds;
        private DataList<RenderCallInfo>.Shared renderCallList;
        private int elementCapacity;

        public event Action<RenderContext> DrawDebugOverlay;
        private LightList<Mesh> batchMeshList;
        private LightList<Mesh> userMeshList;
        private LightList<Material> materialList;
        private StructList<CommandBufferCommand> commandList;
        private JobHandle renderHandle;
        private MaterialPropertyBlock mpb;
        private ThreadSafePool<RenderContext2> renderContextPool;

        internal RenderContextInfo renderContextInfo;

        private PersistentData persistentData;

        private RenderContextInfo collectedRenderInfo;

        public RenderSystem(Application application, LayoutSystem layoutSystem, ElementSystem elementSystem) {
            this.elementSystem = elementSystem;
            this.renderBoxPool = new RenderBoxPool(application.ResourceManager);
            this.layoutSystem = layoutSystem;

            // todo -- ensure we dispose correctly
            this.renderedElementIds = new DataList<ElementId>(128, Allocator.Persistent);
            this.renderCallList = new DataList<RenderCallInfo>.Shared(64, Allocator.Persistent);

            this.batchMeshList = new LightList<Mesh>(32);
            this.materialList = new LightList<Material>();
            this.userMeshList = new LightList<Mesh>();
            this.commandList = new StructList<CommandBufferCommand>();

            this.renderContextPool = new ThreadSafePool<RenderContext2>(
                JobsUtility.MaxJobThreadCount,
                () => new RenderContext2(application.materialDatabase),
                (ctx) => ctx.Clear()
            );

            this.mpb = new MaterialPropertyBlock();

            renderContextInfo = new RenderContextInfo();
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
                        renderBox = new TextRenderBox();
                    }
                    else {
                        renderBox = new StandardRenderBox();
                    }
                }

                renderBoxTable[elementId.index] = renderBox;

                renderInfoTable[elementId] = new RenderInfo() {
                    layer = instance.style.Layer,
                    zIndex = instance.style.ZIndex,
                    drawForeground = renderBox.HasForeground
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
                perViewElementList = new DataList<ElementList>(4, Allocator.Persistent),
                contextPoolHandle = new PerThreadObjectPool<RenderContext2>(renderContextPool),
                renderBoxTableHandle = new GCHandleArray<RenderBox>(renderBoxTable),
                renderContextInfoHandle = new GCHandle<RenderContextInfo>(renderContextInfo),
                renderRanges = new DataList<RangeInt>.Shared(8, Allocator.Persistent),
                contextOffsets = new DataList<ContextOffsets>(JobsUtility.MaxJobThreadCount, Allocator.Persistent),
                drawOrder = new DataList<int>.Shared(32, Allocator.Persistent),
                rangeCount = new HeapAllocated<int>(0),
                drawListSize = new HeapAllocated<int>(0),
                renderCallListSize = new HeapAllocated<int>(0),
                batchList = new DataList<Batch>.Shared(16, Allocator.Persistent),
                batchMemberList = new DataList<int>.Shared(32, Allocator.Persistent),
                shapeBuffer = new PerThread<ShapeDataBuffer>(Allocator.Persistent),
                renderCommands = new DataList<RenderCommand>.Shared(64, Allocator.Persistent),
                perThreadGeometryBuffer = new PerThread<GeometryBuffer>(Allocator.Persistent),
                dummyArray = new NativeArray<int>(1, Allocator.Persistent),
            };
        }

        public virtual void OnUpdate() {

            // todo -- hook this up to the layout system's job scheduling

            persistentData.Clear();
            List<UIView> views = layoutSystem.application.views;

            persistentData.perViewElementList.SetSize(views.Count);

            for (int i = 0; i < views.Count; i++) {

                LayoutContext layoutContext = layoutSystem.GetLayoutContext(views[i]);

                persistentData.perViewElementList[i] = new ElementList() {
                    size = layoutContext.elementList.size,
                    array = layoutContext.elementList.GetArrayPointer()
                };

            }

            JobHandle gatherRenderedElements = VertigoScheduler.Await(new GatherRenderedElements() {
                elementLists = persistentData.perViewElementList,
                traversalTable = elementSystem.traversalTable,
                clipInfoTable = layoutSystem.clipInfoTable,
                renderCallList = renderCallList,
                renderInfoTable = renderInfoTable,
                renderCallListSize = persistentData.renderCallListSize
            });

            // assumes we are able to call all painters in parallel, this miiiight backfire
            JobHandle invokePainters = VertigoScheduler.Await(gatherRenderedElements).ThenDeferParallel(new CallPainters_Managed() {
                    defer = new ParallelParams.Deferred(persistentData.renderCallListSize, 50),
                    renderCallList = renderCallList,
                    matrices = layoutSystem.worldMatrices,
                    renderContextPool = persistentData.contextPoolHandle,
                    renderBoxTableHandle = persistentData.renderBoxTableHandle
                })
                .Then(new MergeRenderContexts_Managed() {
                    drawListSizePtr = persistentData.drawListSize,
                    contextPoolHandle = persistentData.contextPoolHandle,
                    contextOffsets = persistentData.contextOffsets,
                    outputHandle = persistentData.renderContextInfoHandle
                });

            JobHandle renderRange = VertigoScheduler.Await(invokePainters).Then(new GetRenderRangesJobs() {
                drawList = renderContextInfo.drawList,
                drawOrder = persistentData.drawOrder,
                renderRanges = persistentData.renderRanges,
                renderCallList = renderCallList
            });

            // shape baking will happen across threads
            // i need to put the data somewhere
            // one option is to store pointers into per-thread paged lists
            // if more vertex data than page size we're in trouble
            // 2nd option is store pointer base + offset
            JobHandle shapeBaking = VertigoScheduler.Await(invokePainters)
                .ThenDeferParallel(new BakeShapes() {
                    defer = new ParallelParams.Deferred(persistentData.drawListSize, 20),
                    drawList = renderContextInfo.drawList,
                    perThread_ShapeBuffer = persistentData.shapeBuffer
                });

            // await(shapeBaking).Then(ApplyVertexModifiers())

            // this should generate render commands
            // some commands have the type 'batch draw' which is handled by batch
            // can setup a 'pre-req' dependency which ensures batch's clip state is valid before drawing

            JobHandle transparentPass = VertigoScheduler.Await(renderRange)
                .Then(new TransparentRenderPassJob() {
                    drawList = renderContextInfo.drawList,
                    batchList = persistentData.batchList,
                    batchMemberList = persistentData.batchMemberList,
                    renderCommands = persistentData.renderCommands
                    // textureIds = default,
                    // clipInfoTable = layoutSystem.clipInfoTable,
                });

            // renderHandle = JobHandle.CombineDependencies(shapeBaking, transparentPass);

            // todo -- enable parallel deferred by batch list
            renderHandle = VertigoScheduler.Await(transparentPass, shapeBaking).Then(new BuildBatchGeometryJob() {
                drawList = renderContextInfo.drawList,
                batchList = persistentData.batchList,
                batchMemberList = persistentData.batchMemberList,
                perThread_GeometryBuffer = persistentData.perThreadGeometryBuffer
            });

            // need to output render commands and sort them

            // need meshes -> single threaded, but maybe thats ok
            // mesh table already populated
            // just need to convert my already built shape buffers into meshes 
            // this means triangle indices need updating
            // probably part of building the geometry though
            // so the single threaded part is just hydrating the data
            // should already know how many meshes i'll need based purely on vertex count
            // maybe do an allocate meshes pass single threaded then do the rest in parallel
            // can toss them in a list and generate the commands i parallel

            //.Then(new MergeRenderContexts(){});
            //.Then(GetRenderRanges(), BakeShapes(), TransparentRenderPass(), OpaqueRenderPass())
            //.ThenParallel(BuildBatchGeometry()) // may add new batches. we'll need to sort the batch list at the end 
            //.Then(GatherAndSortBatchList(), PopulateBatchMeshes())
            //.Then(GenerateRenderCommands) 

            //todo -- remove sync point

            renderHandle.Complete();

            renderContextInfo.Clear();
            renderContextPool.Clear();
        }

        // this could be parallel except mesh api won't allow it :(
        private void PopulateBatchMeshes() {

            DataList<Batch>.Shared batchList = persistentData.batchList;

            batchMeshList.EnsureCapacity(batchList.size);

            for (int batchIndex = 0; batchIndex < batchList.size; batchIndex++) {
                ref Batch batch = ref batchList[batchIndex];

                if (ReferenceEquals(batchMeshList.array[batchIndex], null)) {
                    batchMeshList.array[batchIndex] = new Mesh();
                    batchMeshList.array[batchIndex].MarkDynamic();
                    batchMeshList.array[batchIndex].indexFormat = IndexFormat.UInt32;
                }

                Mesh mesh = batchMeshList[batchIndex];

                batchMeshList[batchIndex].Clear();

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

        }

        // todo -- get rid of camera, just don't know how to compute the matrices by hand
        public void Render(Camera camera, CommandBuffer commandBuffer) {
            renderHandle.Complete();
            PopulateBatchMeshes();
            renderHandle = default;
            
            // todo -- the only thing I really need from the camera is the matrices
            // I should be able to compute those without a camera reference if I was smarterer
            
            DataList<RenderCommand>.Shared renderCommands = persistentData.renderCommands;
            camera.orthographicSize = Screen.height * 0.5f;
            Matrix4x4 cameraMatrix = camera.cameraToWorldMatrix;
            commandBuffer.SetViewProjectionMatrices(cameraMatrix, camera.projectionMatrix);
            Vector3 cameraOrigin = camera.transform.position;
            cameraOrigin.x -= 0.5f * (Application.UiApplicationSize.width);
            cameraOrigin.y += 0.5f * (Application.UiApplicationSize.height); 
            cameraOrigin.x -= 0.5f;
            cameraOrigin.y += (0.5f); // for some reason editor needs this minor adjustment
            cameraOrigin.z += 2;

            Matrix4x4 origin = Matrix4x4.TRS(cameraOrigin, Quaternion.identity, Vector3.one);
            for (int i = 0; i < renderCommands.size; i++) {
                
                ref RenderCommand cmd = ref renderCommands[i];
                
                switch (cmd.type) {
                    
                    case RenderCommandType.Batch:
                        Mesh mesh = batchMeshList[0];
                        commandBuffer.DrawMesh(mesh, origin, Resources.Load<Material>("UIForiaShape"));
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

            // foreach cmd -> add to buffer
        }

        //
        // public void OnReset() {
        //     commandBuffer.Clear();
        //     for (int i = 0; i < renderOwners.size; i++) {
        //         renderOwners[i].Destroy();
        //     }
        //
        //     renderOwners.QuickClear();
        //     renderContext.clipContext.Destroy();
        //     renderContext.clipContext = new ClipContext(Application.Settings);
        // }

        // public virtual void OnUpdate() {
        //     renderContext.Clear();
        //     // todo
        //     // views can have their own cameras.
        //     // if they do they are not batchable with other views. 
        //     // for now we can make batching not cross view boundaries, eventually that would be cool though
        //
        //     camera.orthographicSize = Screen.height * 0.5f;
        //     for (int i = 0; i < renderOwners.size; i++) {
        //         renderOwners.array[i].Render(renderContext);
        //     }
        //
        //     DrawDebugOverlay2?.Invoke(renderContext);
        //     renderContext.Render(camera, commandBuffer);
        // }

        public void OnDestroy() {

            Dispose();

        }

        public void OnViewAdded(UIView view) {
            // renderOwners.Add(new RenderOwner(view, elementSystem));
        }

        public void OnViewRemoved(UIView view) {
            // for (int i = 0; i < renderOwners.size; i++) {
            //     if (renderOwners.array[i].view == view) {
            //         renderOwners.RemoveAt(i);
            //         return;
            //     }
            // }
        }

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

            renderedElementIds.Dispose();

            renderCallList.Dispose();

            TypedUnsafe.Dispose(renderInfoTable.array, Allocator.Persistent);
            persistentData.Dispose();

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

            public DataList<RangeInt>.Shared renderRanges;
            public DataList<ElementList> perViewElementList;
            public DataList<ContextOffsets> contextOffsets;

            public PerThreadObjectPool<RenderContext2> contextPoolHandle;

            public GCHandleArray<RenderBox> renderBoxTableHandle;
            public GCHandle<RenderContextInfo> renderContextInfoHandle;

            public HeapAllocated<int> rangeCount;
            public HeapAllocated<int> drawListSize;
            public HeapAllocated<int> renderCallListSize;
            public DataList<int>.Shared drawOrder;
            public PerThread<ShapeDataBuffer> shapeBuffer;
            public DataList<Batch>.Shared batchList;
            public DataList<int>.Shared batchMemberList;
            public DataList<RenderCommand>.Shared renderCommands;
            public PerThread<GeometryBuffer> perThreadGeometryBuffer;
            public NativeArray<int> dummyArray;

            public void Dispose() {
                batchList.Dispose();
                perThreadGeometryBuffer.Dispose();
                batchMemberList.Dispose();
                shapeBuffer.Dispose();
                drawOrder.Dispose();
                perViewElementList.Dispose();
                contextOffsets.Dispose();
                contextPoolHandle.Dispose();
                renderBoxTableHandle.Dispose();
                renderContextInfoHandle.Dispose();
                renderRanges.Dispose();
                rangeCount.Dispose();
                drawListSize.Dispose();
                renderCommands.Dispose();
                renderCallListSize.Dispose();
                this = default;
            }

            public void Clear() {
                shapeBuffer.Clear();
                perThreadGeometryBuffer.Clear();
                renderCommands.size = 0;
                batchMemberList.size = 0;
                batchList.size = 0;
                rangeCount.Set(0);
                drawListSize.Set(0);
                renderCallListSize.Set(0);
                renderRanges.size = 0;
                perViewElementList.size = 0;
                drawOrder.size = 0;
            }

        }

    }

    public struct ContextOffsets {

        public int drawListOffset;
        public int propertyOverrideOffset;
        public int textureIdOffset;
        public int transformListOffset;
        public int shapeInfoListOffset;
        public int materialListOffset;
        public int meshListOffset;

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