using System;
using System.Diagnostics;
using UIForia.ListTypes;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia.Graphics {

    public enum RenderCommandType {

        ElementBatch,
        ShapeEffectBatch,
        Mesh,
        MeshBatch,
        SDFTextBatch,
        SDFTextEffectBatch,

        CreateRenderTarget,
        PushRenderTexture,
        ClearRenderTarget,
        MaskAtlasBatch,

        UpdateClipRectBuffer,

        SetClipRectBuffer,

        Callback,

        SetTextDataBuffer,

        SetShapeDatabuffer,

        SetGradientDataBuffer

    }

    public unsafe struct DrawInfoRef {

        public int drawIndex;
        public DrawInfoRef* next;
        public DrawInfoRef* prev;
        public int stencilRequirement;
        public DrawType drawType;
        public AxisAlignedBounds2D aabb;

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
        Element,
        Mesh,
        TextEffect,
        ShapeEffect,

        Mask

    }

    public enum StencilSetupState {

        Uninitialized,
        Pushed,
        Closed,
        Popped

    }
    


    [BurstCompile]
    internal unsafe struct TransparentRenderPassJob : IJob {

        public PagedByteAllocator boundsAllocator;
        public DataList<DrawInfo>.Shared drawList;
        public DataList<Batch>.Shared batchList;
        public DataList<int>.Shared batchMemberList;
        public DataList<RenderCommand>.Shared renderCommands;

        private List_Int32 drawInfoIndices;
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
        private DataList<DrawInfoRef> drawInfoList;

        private bool FullyContainedInClipperBounds() {
            // 
            return false;
        }

        public void Execute3() {
            DrawInfo* drawInfoArray = drawList.GetArrayPointer();

            DataList<StencilInfo> stencilList = new DataList<StencilInfo>();
            List_Int32 clipperStack = new List_Int32(32, Allocator.Temp);
            DrawInfoRef* refList = TypedUnsafe.Malloc<DrawInfoRef>(drawList.size, Allocator.Temp);
            List_Int32 openStencils = new List_Int32(8, Allocator.Temp);

            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo drawInfo = ref drawInfoArray[i];

                refList[i] = new DrawInfoRef() {
                    next = refList + i + 1,
                    prev = refList + i - 1,
                    drawIndex = i,
                    drawType = drawInfo.type,
                    aabb = drawInfo.geometryInfo->bounds,
                    stencilRequirement = clipperStack[clipperStack.size - 1]
                };

                switch (drawInfo.type) {

                    case DrawType.Mesh:
                        break;

                    case DrawType.Shape:
                        break;

                    // cliprect affects batching in that if we have too many of them we cant batch
                    // too many is defined by the shader, if accepts an array of them or not and how big that array is
                    // clip shapes push to the clip rect stack
                    // 
                    // clip behavior handling
                    // need to be a flag on draw info
                    // the clip push would be a new stack that ignores previous ones
                    // so clip stack is really a clip stack of stacks
                    // ctx.SetClippingEnabled(false);
                    // ctx.PushClipScope(rect);
                    // ctx.PushClipRect();

                    // DrawType.PushMaskScope (scopeType == additive | subtractive)
                    // DrawType.PopMaskScope

                    // DrawType.PushMask
                    // DrawType.PopMask

                    case DrawType.PushClipRect:
                        break;

                    case DrawType.PushStencilClip:
                        // if is clip rect -> aabb only 
                        // else use geometry / texture 
                        stencilList.Add(new StencilInfo() {
                            aabb = default,
                            clipperDepth = 0,
                        });
                        break;
                    

         
                }

                if ((drawInfo.type & DrawType.PushStencilClip) != 0) {
                    clipperStack.Add(stencilList.size);
                }
                else if ((drawInfo.type & DrawType.Shape) != 0) { }

                // if totally outside clipper, remove it
                // if removing a clipper itself, anything referring to that clipper is also removed
                while (FullyContainedInClipperBounds()) {
                    // stencilList[idx].drawCount--
                    refList[i].stencilRequirement = stencilList[refList[i].stencilRequirement].parentIndex;
                }

                // aabb == intersection of aabbs 
                // refList[i].aabb -= stencilList[refList[i].stencilRequirement].aabb;
                //if ends up with 0 width or height, remove it

            }

            refList[0].prev = null;
            refList[drawList.size - 1].next = null;

            // i think for now in terms of batching i care that the vertex layout is the same and the material id is the same
            // if there are property overrides or i need to do packing i can figure that out when building batch geometry and can split
            // a single batch into multiple draws there..right?

            // so we have batch and that batch has sub-batches hanging off of it that get processed somewhere else in the pipeline as needed

            int ptr = 0;
            while (ptr != drawList.size) {

                ref DrawInfoRef drawInfoRef = ref refList[ptr];
                ref DrawInfo drawInfo = ref drawList[drawInfoRef.drawIndex];

                if ((drawInfo.type & DrawType.PushStencilClip) != 0) {
                    switch (currentBatchType) {
                        default:
                        case BatchType.None:
                            break;

                        case BatchType.Text:
                            break;

                        case BatchType.Element:
                            EndShapeBatch();
                            // scan through the unrendered list
                            // any shape that would be batch compatible 
                            // check its clip requirements
                            // check its intersections
                            // when we hit stencil I can use that bounds instead of checking all its contents (unless some shape was lifted out of stencil...then i have to check that shape)
                            // if everything is fine
                            // add it to the batch
                            break;

                    }

                    // Begin Stencil Batch
                    // I want to find other stencils that can be batched with the current one
                    // that means non overlapping, in open state, no undrawn & unmasked intersections

                }

                if ((drawInfo.type & DrawType.Shape) != 0) {

                    switch (currentBatchType) {
                        default:
                        case BatchType.None:
                            BeginBatch(BatchType.Element, ptr, ref drawInfo);
                            break;

                        case BatchType.Text:
                            break;

                        case BatchType.Element:
                            if (!CanBatchShape(drawInfo)) {
                                BeginBatch(BatchType.Element, ptr, ref drawInfo);
                            }
                            else {
                                memberBuffer.Add(drawInfoRef.drawIndex);
                            }

                            break;

                    }

                }

                //ptr = drawInfoRef.next;
            }

            // todo -- dispose
            TypedUnsafe.Dispose(refList, Allocator.Temp);

        }

        public void Execute2() {
            DrawInfo* drawInfoArray = drawList.GetArrayPointer();

            DataList<StencilInfo> stencilList = ComputeStencilList();

            List_Int32 candidateIndexList = new List_Int32(32, Allocator.Temp);
            List_Int32 stencilGroupList = new List_Int32(32, Allocator.Temp);

            drawInfoIndices = new List_Int32(32, Allocator.Temp);
            overflowBounds = new PointerList<OverflowBounds>(8, Allocator.Temp);
            memberBuffer = new List_Int32(32, Allocator.Temp);

            for (int i = 0; i < stencilList.size; i++) {

                ref StencilInfo stencilInfo = ref stencilList[i];

             //   if (stencilInfo.isRendered) continue;

                FindStencilsInBatch(i, stencilList, ref stencilGroupList);
                drawInfoIndices.size = 0;

                for (int s = 0; s < stencilGroupList.size; s++) {

                    ref StencilInfo currentStencil = ref stencilList[stencilGroupList[s]];

                    //currentStencil.isRendered = true;

                    // ill need to init the stencil for the batch somewhere, somehow
                    // also not sure how to handle displaying mask, todo for later

                    for (int d = currentStencil.pushIndex; d < currentStencil.popIndex; d++) {

                        ref DrawInfo drawInfo = ref drawInfoArray[d];

                        if ((drawInfo.type & DrawType.PushStencilClip) != 0) {

                            if (d == currentStencil.pushIndex) {
                                continue; // todo -- handle showing mask graphic
                            }

                            int stencilIndex = FindStencilIndexForDrawInfo(stencilList, s, d);

                            // if (!stencilList[stencilIndex].isRendered) {
                            //     drawInfoIndices.Add(d); // if d == push index, only add if we want to render the shape
                            // }

                            d = stencilList[stencilIndex].popIndex;
                            continue;
                        }

            
                        // otherwise its a shape that we want to render. We know it hasn't been rendered yet

                        drawInfoIndices.Add(d);

                    }

                }

                RenderBatches(drawInfoIndices);

            }

            drawInfoIndices.Dispose();
            stencilGroupList.Dispose();
            candidateIndexList.Dispose();
            stencilList.Dispose();

            overflowBounds.Dispose();
            memberBuffer.Dispose();
        }

        private void RenderBatches(in List_Int32 indices) {

            // improvement: if no clip texture is used and clip shape is known (bevel, rounded, rect) it might be possible to skip the clipper if nothing is actually overflowing
            // or we can take the non overflowing elements and lift them a level higher, would need some kind of adoption buffer for that
            // then if draw count for a stencil was 0, we could remove it

            // none of the stencils we are drawing into overlap, and we know none shapes can be drawn outside the bounds of their stencil
            // still need to know which stencil each draw refers to because ill need to cull pixels outside of its stencil's aligned rect
            // if i dont use that feature then need to draw more, can still support what i want to do but cant allow shapes to intersect
            // any stencil other than their originating one

            StencilState stencilState = new StencilState();
            stencilState.enabled = true;
            stencilState.readMask = 255;
            stencilState.writeMask = 255;
            stencilState.compareFunctionFront = CompareFunction.Greater;
            stencilState.compareFunctionBack = CompareFunction.Greater;
            stencilState.failOperationFront = StencilOp.Keep;
            memberBuffer.size = 0;

            int size = indices.size;
            DrawInfo* drawInfoArray = drawList.GetArrayPointer();

            for (int i = 0; i < size; i++) {

                int drawIdx = indices.array[i];
                ref DrawInfo drawInfo = ref drawInfoArray[drawIdx];

                if ((drawInfo.flags & DrawInfoFlags.BatchSet) != 0) {
                    continue;
                }

                // for now dont draw mask
                if ((drawInfo.type & DrawType.PushStencilClip) != 0) {
                    continue;
                }

                if ((drawInfo.type & DrawType.Shape) != 0) {

                    switch (currentBatchType) {
                        default:
                        case BatchType.None:
                            BeginBatch(BatchType.Element, drawIdx, ref drawInfo);
                            break;

                        case BatchType.Element when !CanBatchShape(drawInfo):
                            BeginBatch(BatchType.Element, drawIdx, ref drawInfo);
                            continue;

                        case BatchType.Element:
                            memberBuffer.Add(drawIdx);
                            break;
                    }

                }

            }

            EndBatch();

        }

        private static int FindStencilIndexForDrawInfo(in DataList<StencilInfo> stencilList, int start, int targetDrawInfoIndex) {
            for (int i = start; i < stencilList.size; i++) {
                if (stencilList[i].pushIndex == targetDrawInfoIndex) {
                    return i;
                }
            }

            return 0; // should never hit this
        }

        private static void FindStencilsInBatch(int startIdx, in DataList<StencilInfo> stencilList, ref List_Int32 output) {

            int depthTarget = stencilList[startIdx].clipperDepth;

            AxisAlignedBounds2D aabb = stencilList[startIdx].aabb;
            output.size = 0;

            output.Add(startIdx);

            // find unrendered stencils that are at the same depth as this one
            for (int i = startIdx + 1; i < stencilList.size; i++) {

                ref StencilInfo stencilCheck = ref stencilList[i];

                // if (stencilCheck.isRendered) {
                //     continue;
                // }

                if (stencilCheck.clipperDepth != depthTarget) {
                    continue;
                }

                AxisAlignedBounds2D testBounds = stencilCheck.aabb;

                bool overlappingOrContains = aabb.xMax > testBounds.xMin && aabb.xMin < testBounds.xMax && aabb.yMax > testBounds.yMin && aabb.yMin < testBounds.yMax;

                if (!overlappingOrContains) {
                 //   stencilCheck.isRendered = true;
                    output.Add(i);
                }

            }
        }

        private DataList<StencilInfo> ComputeStencilList() {

            int size = drawList.size;
            DrawInfo* drawInfoArray = drawList.GetArrayPointer();

            DataList<StencilInfo> stencilList = new DataList<StencilInfo>(16, Allocator.Temp);
            DataList<int> stencilStack = new DataList<int>(16, Allocator.Temp);

            stencilStack.Add(0);
            stencilList.Add(new StencilInfo() {
                aabb = new AxisAlignedBounds2D(0, 0, 2048, 2048), // todo -- screen/surface
                clipperDepth = 0,
                parentIndex = -1,
                popIndex = drawList.size,
                pushIndex = 0
            });

            for (int i = 0; i < size; i++) {

                ref DrawInfo drawInfo = ref drawInfoArray[i];

                if ((drawInfo.type & DrawType.PushStencilClip) != 0) {
                    stencilList.Add(new StencilInfo() {
                        aabb = drawInfo.geometryInfo->bounds,
                        clipperDepth = stencilStack.size,
                        parentIndex = stencilStack[stencilStack.size - 1],
                        popIndex = -1,
                        pushIndex = i
                    });
                    stencilStack.Add(stencilList.size - 1);
                }
                // else if ((drawInfo.type & DrawType.PopClipShape) != 0) {
                //     int lastIdx = stencilStack.GetLast();
                //     ref StencilInfo stencil = ref stencilList[lastIdx];
                //     stencil.popIndex = i;
                //     stencilStack.size--;
                // }

            }

            for (int i = 0; i < stencilStack.size; i++) {
                int idx = stencilStack[i];
                ref StencilInfo stencil = ref stencilList[idx];
                stencil.popIndex = drawList.size;
            }

            return stencilList;
        }

        // if not using texture stencil and I can verify that a shape is 100% contained by its stencil, it can be added to a different stencil group too

        public void Execute() {

            int size = drawList.size;
            overflowBounds = new PointerList<OverflowBounds>(8, Allocator.Temp);
            memberBuffer = new List_Int32(32, Allocator.Temp);

            DrawInfo* drawInfoArray = drawList.GetArrayPointer();
            for (int i = 0; i < size; i++) {

                ref DrawInfo drawInfo = ref drawInfoArray[i];

                if ((drawInfo.flags & (DrawInfoFlags.BatchSet | DrawInfoFlags.FinalBatchSet)) != 0) {
                    continue;
                }

                if ((drawInfo.shapeType == ShapeType.SDFText)) {
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
                        BeginBatch(BatchType.Element, i, ref drawInfo);
                    }
                    else if (currentBatchType == BatchType.Element) {

                        if (!CanBatchShape(drawInfo)) {
                            EndBatch();
                            BeginBatch(BatchType.Element, i, ref drawInfo);
                            continue;
                        }

                        memberBuffer.Add(i);

                    }
                    else {
                        EndBatch();
                        BeginBatch(BatchType.Element, i, ref drawInfo);
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

                if ((drawInfo.flags & (DrawInfoFlags.BatchSet | DrawInfoFlags.FinalBatchSet)) != 0) {
                    continue;
                }

              
                if (!IsKnownMaterial(drawInfo.materialId)) {
                    break;
                }

                if (CanBatchSDFText(drawInfo) && IntersectionTest(drawInfo, lastInBatch + 1, i)) {
                    drawInfo.flags |= DrawInfoFlags.BatchSet;
                    memberBuffer.Add(i);
                }

            }

        }

        // configure this with a max search limit
        private void OutOfOrderBatchShape() {

            int lastInBatch = memberBuffer[memberBuffer.size - 1];

            // + 1 broke the batch, so start at + 2
            for (int i = lastInBatch + 2; i < drawInfoIndices.size; i++) {

                ref DrawInfo drawInfo = ref drawList[drawInfoIndices[i]];

                if ((drawInfo.flags & DrawInfoFlags.FinalBatchSet) != 0) {
                    continue;
                }

                if (!IsKnownMaterial(drawInfo.materialId)) {
                    break;
                }

                if (CanBatchShape(drawInfo) && IntersectionTest(drawInfo, lastInBatch + 1, i)) {
                    drawInfo.flags |= DrawInfoFlags.BatchSet;
                    memberBuffer.Add(i);
                }

            }

        }

        private void EndBatch() {
            switch (currentBatchType) {

                case BatchType.Text:
                    EndTextBatch();
                    break;

                case BatchType.Element: {
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

                // if (firstInBatch.overflowBounds != null) {
                //     firstInBatch.overflowBoundRenderIndex = 0;
                //     overflowBounds.Add(firstInBatch.overflowBounds);
                // }
                // else {
                //     firstInBatch.overflowBoundRenderIndex = -1;
                // }

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

                    // if (drawInfo.overflowBounds != null) {
                    //
                    //     int boundsIndex = -1;
                    //
                    //     for (int overflowIdx = 0; overflowIdx < overflowBounds.size; overflowIdx++) {
                    //
                    //         if (drawInfo.overflowBounds == overflowBounds[overflowIdx]) {
                    //             drawInfo.overflowBoundRenderIndex = overflowIdx;
                    //             boundsIndex = overflowIdx;
                    //             break;
                    //         }
                    //
                    //     }
                    //
                    //     if (boundsIndex == -1 && overflowBounds.size + 1 < k_MaxOverflowBounds) {
                    //         drawInfo.overflowBoundRenderIndex = overflowBounds.size;
                    //         overflowBounds.Add(drawInfo.overflowBounds);
                    //     }
                    //     else if (boundsIndex == -1) {
                    //         continue;
                    //     }
                    //
                    // }

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
            EndBatch();
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

                // if (firstInBatch.overflowBounds != null) {
                //     firstInBatch.overflowBoundRenderIndex = 0;
                //     overflowBounds.Add(firstInBatch.overflowBounds);
                // }
                // else {
                //     firstInBatch.overflowBoundRenderIndex = -1;
                // }

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

                    // if (drawInfo.overflowBounds != null) {
                    //
                    //     int boundsIndex = -1;
                    //
                    //     for (int overflowIdx = 0; overflowIdx < overflowBounds.size; overflowIdx++) {
                    //
                    //         if (drawInfo.overflowBounds == overflowBounds[overflowIdx]) {
                    //             drawInfo.overflowBoundRenderIndex = overflowIdx;
                    //             boundsIndex = overflowIdx;
                    //             break;
                    //         }
                    //
                    //     }
                    //
                    //     if (boundsIndex == -1 && overflowBounds.size + 1 < k_MaxOverflowBounds) {
                    //         drawInfo.overflowBoundRenderIndex = overflowBounds.size;
                    //         overflowBounds.Add(drawInfo.overflowBounds);
                    //     }
                    //     else if (boundsIndex == -1) {
                    //         continue;
                    //     }  
                    //
                    // }

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
                // batchType = BatchType.Text, // todo -- or text effect!
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
                type = RenderCommandType.ElementBatch,
                batchIndex = batchList.size
            });

            batchList.Add(new Batch() {
                vertexLayout = vertexLayout,
                // batchType = BatchType.Shape,
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

            AxisAlignedBounds2D aabb = drawInfo.geometryInfo->bounds;

            for (int i = orderedBatchEndIndex + 1; i < testIndex; i++) {
                ref DrawInfo test = ref drawList[drawInfoIndices[i]];

                if ((test.flags & DrawInfoFlags.BatchSet) != 0 || (test.type & DrawType.Shape) == 0) {
                    continue;
                }

                AxisAlignedBounds2D testBounds = test.geometryInfo->bounds;

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