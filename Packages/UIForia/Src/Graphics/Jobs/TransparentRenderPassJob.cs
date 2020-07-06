using UIForia.ListTypes;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    public enum RenderCommandType {

        ShapeBatch,
        ShapeEffectBatch,
        Mesh,
        MeshBatch,
        SDFTextBatch,
        SDFTextEffectBatch,

        CreateRenderTarget,
        PushRenderTexture,
        ClearRenderTarget,
        MaskAtlasBatch

    }

    public unsafe struct RenderCommand {

        public RenderCommandType type;
        public int batchIndex;
        public int meshIndex;
        public void* data; // context dependent. for batches this is float4 list for clipping 
        public int sortIdx;

    }

    public enum BatchType {

        None,
        Text,
        Shape,
        Mesh,
        TextEffect,
        ShapeEffect,

        Mask

    }

    [BurstCompile]
    internal unsafe struct TransparentRenderPassJob : IJob {

        public PagedByteAllocator boundsAllocator;
        public DataList<DrawInfo>.Shared drawList;
        public DataList<Batch>.Shared batchList;
        public DataList<int>.Shared batchMemberList;
        public DataList<RenderCommand>.Shared renderCommands;

        private BatchType currentBatchType;
        private int activeMaterialId;

        private VertexLayout currentVertexLayout;

        private int activeMaterialOverrideCount;
        private bool activeMaterialCanBatch;
        private int lastRenderedId;

        // todo -- move these to the batch splitter
        private PointerList<OverflowBounds> overflowBounds;
        private List_Int32 memberBuffer;
        [NativeDisableUnsafePtrRestriction] private MaterialPropertyOverride* materialOverrides;

        private const int k_MaxOverflowBounds = 4;

        public void Execute() {

            int size = drawList.size;
            overflowBounds = new PointerList<OverflowBounds>(8, Allocator.Temp);
            memberBuffer = new List_Int32(32, Allocator.Temp);

            DrawInfo* drawInfoArray = drawList.GetArrayPointer();
            for (int i = 0; i < size; i++) {

                ref DrawInfo drawInfo = ref drawInfoArray[i];

                if ((drawInfo.flags & (DrawInfoFlags.InitialBatchSet | DrawInfoFlags.FinalBatchSet)) != 0) {
                    continue;
                }

                if ((drawInfo.type == DrawType.SDFText)) {
                    if (currentBatchType == BatchType.None) {
                        BeginBatch(BatchType.Text, i, ref drawInfo);
                    }
                    else if (currentBatchType == BatchType.Text) {

                        if (!CanBatchSDFText(drawInfo)) {
                            EndBatch();
                            BeginBatch(BatchType.Text, i, ref drawInfo);
                            continue;
                        }

                        memberBuffer.Add(i);

                    }
                    else {
                        EndBatch();
                        BeginBatch(BatchType.Text, i, ref drawInfo);
                    }
                }
                else if ((drawInfo.type & DrawType.Shape) != 0) {

                    if (currentBatchType == BatchType.None) {
                        BeginBatch(BatchType.Shape, i, ref drawInfo);
                    }
                    else if (currentBatchType == BatchType.Shape) {

                        if (!CanBatchShape(drawInfo)) {
                            EndBatch();
                            BeginBatch(BatchType.Shape, i, ref drawInfo);
                            continue;
                        }

                        memberBuffer.Add(i);

                    }
                    else {
                        EndBatch();
                        BeginBatch(BatchType.Shape, i, ref drawInfo);
                    }

                }
            }

            EndBatch();

            overflowBounds.Dispose();
            memberBuffer.Dispose();

        }

        private bool CanBatchSDFText(DrawInfo drawInfo) {

            if (drawInfo.materialId.index != activeMaterialId) {
                return false;
            }

            // this first pass just checks material id, later I'll output something that is bound with material properties

            return true;
        }

        private void OutOfOrderBatchSDFText() {

            int lastInBatch = memberBuffer[memberBuffer.size - 1];

            // + 1 broke the batch, so start at + 2
            for (int i = lastInBatch + 2; i < drawList.size; i++) {

                ref DrawInfo drawInfo = ref drawList[i];

                if ((drawInfo.flags & (DrawInfoFlags.InitialBatchSet | DrawInfoFlags.FinalBatchSet)) != 0) {
                    continue;
                }

                if ((drawInfo.type & DrawType.StateChange) != 0) {
                    break;
                }

                if (!IsKnownMaterial(drawInfo.materialId)) {
                    break;
                }

                if (CanBatchSDFText(drawInfo) && IntersectionTest(drawInfo, lastInBatch + 1, i)) {
                    drawInfo.flags |= DrawInfoFlags.InitialBatchSet;
                    memberBuffer.Add(i);
                }

            }

        }

        // configure this with a max search limit
        private void OutOfOrderBatchShape() {

            int lastInBatch = memberBuffer[memberBuffer.size - 1];

            // + 1 broke the batch, so start at + 2
            for (int i = lastInBatch + 2; i < drawList.size; i++) {

                ref DrawInfo drawInfo = ref drawList[i];

                if ((drawInfo.flags & DrawInfoFlags.FinalBatchSet) != 0) {
                    continue;
                }

                if ((drawInfo.type & DrawType.StateChange) != 0) {
                    break;
                }

                if (!IsKnownMaterial(drawInfo.materialId)) {
                    break;
                }

                if (CanBatchShape(drawInfo) && IntersectionTest(drawInfo, lastInBatch + 1, i)) {
                    drawInfo.flags |= DrawInfoFlags.InitialBatchSet;
                    memberBuffer.Add(i);
                }

            }

        }

        private void EndBatch() {
            switch (currentBatchType) {

                case BatchType.Text:
                    EndTextBatch();
                    break;

                case BatchType.Shape: {
                    EndShapeBatch();
                    break;
                }

                case BatchType.Mesh:
                    break;

                case BatchType.None:
                    break;

            }

            currentBatchType = BatchType.None;
            memberBuffer.size = 0;
            overflowBounds.size = 0;
        }
        
        // we have a few types of text batching
        // 1 -> by material property where we directly override shader properties
        // 2 -> by packing data in vertices 

        public struct CharacterVertex {

            public int type;
            public float3 position;
            public float2 texCoord;
            public float2 faceCoord;
            public Color32 faceColor;
            public Color32 outlineColor;
            public Color32 glowColor;
            public Color32 underlayColor;
        }
        
        private void EndTextBatch() {

            OutOfOrderBatchSDFText();

            int size = memberBuffer.size;
            int start = batchMemberList.size;

            int vertexCount = 0;
            
            // scan all candidates
            // memcmp their text style buffers
            // if they are the same and pass intersection test, add to currrent batch
            // if they not, continue
            // if they have material properties set, can't batch (I think)
            // for known text material, ignore previously set material properties, pretend they didnt apply
            // we want to control everything through textStyle so the batcher works

            // text feature material id
            // if feature sets dont match, cannot batch, no reason to evaluate further
            // if feature set is texture based, must use same textures
            // if feature set includes bevel -> ???
            // if feature set includes effect -> ?? 
            // if color buffer is full, break
            // if clip buffer is full, break
            
            // ill need to set color buffer indices somehow
            // need to store which colors are needed
            // to support quad coloring ill need more data? no just more indices, still need per vertex data, let gpu extrapolate
            
            // instead of setting texcoord 1 in baker, let batcher handle it
            // just need to know the used colors per draw info 
            // then go back and map indices
            // how? 
            // also supporting various underline colorings
            // i guess each vertex has a color value
            // ok, how do i remap it?
            // foreach color
            //     baker writes an index instead of a color to the byte
            //     then i just build a mapping table of index->new index
            //     remapping isnt super fast id think
            //     needs to happen after effects? probalby not, just dont let text have effects in the traditional sense
            //     or have text baking return vertices with a type
            //     
            // do we ever render in non batched mode? for now probably not
            
            // color management options
                // i want to support color quads I think
                // if doing that i need to put the 4 colors in each slot of the colors value
                // then every 4 vertices set the color
                // this breaks for underline / strike through
                // maybe a gradient is just better?
                // otherwise i need to somehow cpu interpolate between vertex colors and it would look weird
                // ok so no color quads
                // better to pack 1 byte into each color slot
                // i need map of colors in the pallet probably
                // draw info has list of spans
                // foreach span get colors
                // dont bother assigning to vertex colors 
                // doing this makes text effects unlikely
                // maybe text effect handles colors differently or exposes some api
                
                // GetTextRenderInfo().SetOutlineColor(red);
                
            // where do i compute other data for text?
                // glow, underline
                // in the geometry batcher?
                // someone has to decide where the data goes
                // maybe best for baker to make the mesh, set uv0 
                // all other effect based stuff added later
                // mesh batcher determines packing, can batcher split batches?
                // at this point effects have run already I think
                // if text was manipulated exclude from batching
                // i like the batch geometry builder doing batching because it can be threaded while this job can't be
                
            
            for (int x = 0; x < size; x++) {

                int drawIndex = memberBuffer[x];

                ref DrawInfo firstInBatch = ref drawList[drawIndex];

                if ((firstInBatch.flags & DrawInfoFlags.FinalBatchSet) != 0) {
                    continue;
                }

                if (firstInBatch.overflowBounds != null) {
                    firstInBatch.overflowBoundRenderIndex = 0;
                    overflowBounds.Add(firstInBatch.overflowBounds);
                }
                else {
                    firstInBatch.overflowBoundRenderIndex = -1;
                }

                firstInBatch.flags |= DrawInfoFlags.FinalBatchSet;

                RangeInt currentBatchRange = new RangeInt(batchMemberList.size, 1);

                batchMemberList.Add(drawIndex);

                // todo -- improve this
                
                activeMaterialOverrideCount = firstInBatch.materialOverrideCount;
                materialOverrides = firstInBatch.materialOverrideValues;

                vertexCount += firstInBatch.geometryInfo->vertexCount;
                
                for (int i = x + 1; i < size; i++) {

                    // todo -- ill need do intersection tests with all non assigned elements inorder for this to work

                    ref DrawInfo drawInfo = ref drawList[memberBuffer.array[i]];

                    if ((drawInfo.flags & DrawInfoFlags.FinalBatchSet) != 0) {
                        continue;
                    }

                    if (drawInfo.materialOverrideCount != activeMaterialOverrideCount) {
                        continue;
                    }

                    if ((drawInfo.flags & DrawInfoFlags.HasNonTextureOverrides) != 0) {
                        continue;
                    }

                    bool texturesMatch = true;
                    for (int overrideIdx = 0; overrideIdx < activeMaterialOverrideCount; overrideIdx++) {
                        ref MaterialPropertyOverride overrideVal = ref drawInfo.materialOverrideValues[overrideIdx];
                        ref MaterialPropertyOverride currentVal = ref materialOverrides[overrideIdx];
                        if (overrideVal.shaderPropertyId != currentVal.shaderPropertyId || overrideVal.value.textureId != currentVal.value.textureId) {
                            texturesMatch = false;
                            break;
                        }
                    }

                    if (!texturesMatch) {
                        continue;
                    }

                    if (drawInfo.overflowBounds != null) {

                        int boundsIndex = -1;

                        for (int overflowIdx = 0; overflowIdx < overflowBounds.size; overflowIdx++) {

                            if (drawInfo.overflowBounds == overflowBounds[overflowIdx]) {
                                drawInfo.overflowBoundRenderIndex = overflowIdx;
                                boundsIndex = overflowIdx;
                                break;
                            }

                        }

                        if (boundsIndex == -1 && overflowBounds.size + 1 < k_MaxOverflowBounds) {
                            drawInfo.overflowBoundRenderIndex = overflowBounds.size;
                            overflowBounds.Add(drawInfo.overflowBounds);
                        }
                        else if (boundsIndex == -1) {
                            continue;
                        }

                    }

                    drawInfo.flags |= DrawInfoFlags.FinalBatchSet;
                    currentBatchRange.length++;
                    batchMemberList.Add(memberBuffer.array[i]);

                   // if (vertexCount >= k_TextVertexLimit) {
                   //     // EndShapeBatchSection(firstInBatch.vertexLayout, currentBatchRange);
                   //     vertexCount = 0;
                   //     break;
                   // }
                    
                }

                EndShapeBatchSection(firstInBatch.vertexLayout, currentBatchRange);

            }

            // renderCommands.Add(new RenderCommand() {
            //     type = RenderCommandType.ShapeBatch,
            //     batchIndex = batchList.size
            // });
            //
            // ref DrawInfo firstInBatch = ref drawList[memberBuffer[0]];
            //
            // batchMemberList.AddRange(memberBuffer.array, memberBuffer.size);
            // activeMaterialOverrideCount = firstInBatch.materialOverrideCount;
            // materialOverrides = firstInBatch.materialOverrideValues;
            //
            // batchList.Add(new Batch() {
            //     vertexLayout = currentVertexLayout,
            //     batchType = BatchType.Text,
            //     memberIdRange = new RangeInt(start, size),
            //     materialId = new MaterialId(activeMaterialId),
            //     propertyOverrides = materialOverrides,
            //     propertyOverrideCount = activeMaterialOverrideCount
            // });
            //
            // if (overflowBounds.size > 0) {
            //     OverflowBounds* orientedBoundsPtr = boundsAllocator.Allocate<OverflowBounds>(overflowBounds.size);
            //     for (int i = 0; i < overflowBounds.size; i++) {
            //         orientedBoundsPtr[i] = *overflowBounds[i];
            //     }
            // }

        }

        private bool CanBatchShape(in DrawInfo drawInfo) {
            if (!activeMaterialCanBatch || drawInfo.materialId.index != activeMaterialId) {
                return false;
            }

            return (drawInfo.flags & DrawInfoFlags.HasNonTextureOverrides) == 0;
        }

        private void BeginBatch(BatchType batchType, int idx, ref DrawInfo drawInfo) {
            currentBatchType = batchType;
            currentVertexLayout = drawInfo.vertexLayout;
            activeMaterialId = drawInfo.materialId.index;
            memberBuffer.Add(idx);
            activeMaterialCanBatch = (drawInfo.flags & DrawInfoFlags.HasMaterialOverrides) == 0 || (drawInfo.flags & DrawInfoFlags.HasNonTextureOverrides) == 0;
        }

        private void EndShapeBatch() {

            if (activeMaterialCanBatch) {
                OutOfOrderBatchShape();
            }

            int size = memberBuffer.size;
            for (int x = 0; x < size; x++) {

                int drawIndex = memberBuffer[x];

                ref DrawInfo firstInBatch = ref drawList[drawIndex];

                if ((firstInBatch.flags & DrawInfoFlags.FinalBatchSet) != 0) {
                    continue;
                }

                if (firstInBatch.overflowBounds != null) {
                    firstInBatch.overflowBoundRenderIndex = 0;
                    overflowBounds.Add(firstInBatch.overflowBounds);
                }
                else {
                    firstInBatch.overflowBoundRenderIndex = -1;
                }

                firstInBatch.flags |= DrawInfoFlags.FinalBatchSet;

                RangeInt currentBatchRange = new RangeInt(batchMemberList.size, 1);

                batchMemberList.Add(drawIndex);

                activeMaterialOverrideCount = firstInBatch.materialOverrideCount;
                materialOverrides = firstInBatch.materialOverrideValues;

                if ((firstInBatch.flags & DrawInfoFlags.HasNonTextureOverrides) != 0) {
                    EndShapeBatchSection(firstInBatch.vertexLayout, currentBatchRange);
                    continue;
                }

                for (int i = x + 1; i < size; i++) {

                    // todo -- ill need do intersection tests with all non assigned elements inorder for this to work

                    ref DrawInfo drawInfo = ref drawList[memberBuffer.array[i]];

                    if ((drawInfo.flags & DrawInfoFlags.FinalBatchSet) != 0) {
                        continue;
                    }

                    if (drawInfo.materialOverrideCount != activeMaterialOverrideCount) {
                        continue;
                    }

                    if ((drawInfo.flags & DrawInfoFlags.HasNonTextureOverrides) != 0) {
                        continue;
                    }

                    bool texturesMatch = true;
                    for (int overrideIdx = 0; overrideIdx < activeMaterialOverrideCount; overrideIdx++) {
                        ref MaterialPropertyOverride overrideVal = ref drawInfo.materialOverrideValues[overrideIdx];
                        ref MaterialPropertyOverride currentVal = ref materialOverrides[overrideIdx];
                        if (overrideVal.shaderPropertyId != currentVal.shaderPropertyId || overrideVal.value.textureId != currentVal.value.textureId) {
                            texturesMatch = false;
                            break;
                        }
                    }

                    if (!texturesMatch) {
                        continue;
                    }

                    if (drawInfo.overflowBounds != null) {

                        int boundsIndex = -1;

                        for (int overflowIdx = 0; overflowIdx < overflowBounds.size; overflowIdx++) {

                            if (drawInfo.overflowBounds == overflowBounds[overflowIdx]) {
                                drawInfo.overflowBoundRenderIndex = overflowIdx;
                                boundsIndex = overflowIdx;
                                break;
                            }

                        }

                        if (boundsIndex == -1 && overflowBounds.size + 1 < k_MaxOverflowBounds) {
                            drawInfo.overflowBoundRenderIndex = overflowBounds.size;
                            overflowBounds.Add(drawInfo.overflowBounds);
                        }
                        else if (boundsIndex == -1) {
                            continue;
                        }

                    }

                    drawInfo.flags |= DrawInfoFlags.FinalBatchSet;
                    currentBatchRange.length++;
                    batchMemberList.Add(memberBuffer.array[i]);
                }

                EndShapeBatchSection(firstInBatch.vertexLayout, currentBatchRange);

            }

        }

        private void EndTextBatchSection(RenderCommandType commandType, VertexLayout vertexLayout, RangeInt currentBatchRange) {
            renderCommands.Add(new RenderCommand() {
                type = commandType,
                batchIndex = batchList.size
            });

            batchList.Add(new Batch() {
                vertexLayout = vertexLayout,
                batchType = BatchType.Text, // todo -- or text effect!
                memberIdRange = currentBatchRange,
                materialId = new MaterialId(activeMaterialId),
                propertyOverrides = materialOverrides,
                propertyOverrideCount = activeMaterialOverrideCount
            });

            if (overflowBounds.size > 0) {
                OverflowBounds* orientedBoundsPtr = boundsAllocator.Allocate<OverflowBounds>(overflowBounds.size);
                for (int i = 0; i < overflowBounds.size; i++) {
                    orientedBoundsPtr[i] = *overflowBounds[i];
                }
            }

            overflowBounds.size = 0;
        }

        private void EndShapeBatchSection(VertexLayout vertexLayout, RangeInt currentBatchRange) {

            renderCommands.Add(new RenderCommand() {
                type = RenderCommandType.ShapeBatch,
                batchIndex = batchList.size
            });

            batchList.Add(new Batch() {
                vertexLayout = vertexLayout,
                batchType = BatchType.Shape,
                memberIdRange = currentBatchRange,
                materialId = new MaterialId(activeMaterialId),
                propertyOverrides = materialOverrides,
                propertyOverrideCount = activeMaterialOverrideCount
            });

            if (overflowBounds.size > 0) {
                OverflowBounds* orientedBoundsPtr = boundsAllocator.Allocate<OverflowBounds>(overflowBounds.size);
                for (int i = 0; i < overflowBounds.size; i++) {
                    orientedBoundsPtr[i] = *overflowBounds[i];
                }
            }

            overflowBounds.size = 0;
        }

        /// <summary>
        /// When we encounter the end of an 'in-order' batch, we can continue to add more elements to that batch,
        /// as long as their aabb doesn't intersect with anything that hasn't yet been rendered
        /// </summary>
        /// <param name="drawInfo"></param>
        /// <param name="orderedBatchEndIndex"></param>
        /// <param name="testIndex"></param>
        /// <returns></returns>
        private bool IntersectionTest(in DrawInfo drawInfo, int orderedBatchEndIndex, int testIndex) {

            AxisAlignedBounds2D aabb = drawInfo.aabb;

            for (int i = orderedBatchEndIndex + 1; i < testIndex; i++) {
                ref DrawInfo test = ref drawList[i];

                if ((test.flags & DrawInfoFlags.InitialBatchSet) != 0 || (test.type & DrawType.Shape) == 0) {
                    continue;
                }

                AxisAlignedBounds2D testBounds = test.aabb;

                // maybe can be done with simd bool4 by replacing && with &
                bool overlappingOrContains = aabb.xMax > testBounds.xMin && aabb.xMin < testBounds.xMax && aabb.yMax > testBounds.yMin && aabb.yMin < testBounds.yMax;

                if (overlappingOrContains) {
                    return false;
                }

            }

            return true;
        }

        /// <summary>
        /// If the material in question might change render state or might mutate vertices, we cannot batch across this draw call
        /// </summary>
        /// <param name="candidateMaterialId"></param>
        /// <returns></returns>
        private bool IsKnownMaterial(MaterialId candidateMaterialId) {
            return true;
        }

    }

}