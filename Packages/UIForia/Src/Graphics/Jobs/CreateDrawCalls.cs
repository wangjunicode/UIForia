using System;
using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct CreateDrawCalls : IJob {

        public int floatBufferSize;

        public DataList<DrawInfo>.Shared drawList;
        public DataList<ProcessedDrawInfo>.Shared processedDrawList;
        public DataList<StencilInfo>.Shared stencilList;
        public DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;
        public DataList<Batch>.Shared batchList;
        public DataList<int>.Shared batchMemberList;
        public DataList<RenderCommand>.Shared renderCommands;

        private BatchType currentBatchType;
        private List_Int32 clipperIdList;
        private List_Int32 stencilIdList;
        private List_Int32 inOrderBatchList;
        private List_Int32 outOfOrderBatchList;
        private List_Int32 stencilsToPush;
        private List_Int32 stencilsToPop;
        public PagedByteAllocator boundsAllocator;

        public const int k_MaxClipRects = 64;
        public const int k_TextDataBufferSize = 256;
        public const int k_ShapeDataBufferSize = 256;
        
        private int clipRectCount;
        private AxisAlignedBounds2D* clipRectBuffer;
        
        public void Execute() {

            renderCommands.Add(new RenderCommand() {
                type = RenderCommandType.SetClipRectBuffer,
                data = boundsAllocator.Allocate<AxisAlignedBounds2D>(k_MaxClipRects)
            });
            
            renderCommands.Add(new RenderCommand() {
                type = RenderCommandType.SetTextDataBuffer,
                data = boundsAllocator.Allocate<AxisAlignedBounds2D>(k_MaxClipRects)
            });
            
            renderCommands.Add(new RenderCommand() {
                type = RenderCommandType.SetShapeDatabuffer,
                data = boundsAllocator.Allocate<AxisAlignedBounds2D>(k_MaxClipRects)
            });
            
            renderCommands.Add(new RenderCommand() {
                type = RenderCommandType.SetGradientDataBuffer,
                data = boundsAllocator.Allocate<AxisAlignedBounds2D>(k_MaxClipRects)
            });
            
            int ptr = 0;

            bool allowOutOfOrderBatching = false;

            inOrderBatchList = new List_Int32(128, Allocator.Temp);
            outOfOrderBatchList = new List_Int32(128, Allocator.Temp);
            clipperIdList = new List_Int32(16, Allocator.Temp);
            stencilsToPop = new List_Int32(16, Allocator.Temp);
            stencilsToPush = new List_Int32(16, Allocator.Temp);

            while (ptr < processedDrawList.size && ptr >= 0) {

                ref ProcessedDrawInfo info = ref processedDrawList[ptr];

                if (info.type == DrawType.Callback) {
                    // Break Batch();
                    renderCommands.Add(new RenderCommand() {
                        type = RenderCommandType.Callback,
                        data = drawList[info.drawInfoIndex].shapeData // assume this points us to the callback
                    });
                    continue;
                }
                
                if (info.type == DrawType.Shape) {
                    ptr = UIForiaShapeBatch_InOrderBatch(ptr);
                    if (allowOutOfOrderBatching) {
                        // UIForiaShapeBatch_OutOfOrderBatch();
                    }

                    IssueStencilUpdate();

                    RangeInt batchRange = new RangeInt(batchMemberList.size, inOrderBatchList.size + outOfOrderBatchList.size);
                    AxisAlignedBounds2D* clipRects = null;

                    if (clipperIdList.size > 0) {
                        clipRects = boundsAllocator.Allocate<AxisAlignedBounds2D>(clipperIdList.size);
                        for (int i = 0; i < clipperIdList.size; i++) {
                            clipRects[i] = clipperBoundsList[clipperIdList[i]];
                        }
                    }

                    Batch batchInfo = new Batch() {
                        clipRects = clipRects,
                        clipRectCount = clipperIdList.size,
                        colorMask = ColorWriteMask.All,
                        materialId = info.materialId,
                        propertyOverrides = drawList[info.drawInfoIndex].materialOverrideValues,
                        propertyOverrideCount = drawList[info.drawInfoIndex].materialOverrideCount,

                        stencilRefValue = stencilList[info.stencilIndex].childRefValue,
                        stencilState = stencilList[info.stencilIndex].state,

                        vertexLayout = info.vertexLayout,
                        memberIdRange = batchRange,
                    };

                    renderCommands.Add(new RenderCommand() {
                        type = RenderCommandType.ElementBatch,
                        batchIndex = batchList.size
                    });

                    batchMemberList.AddRange(inOrderBatchList.array, inOrderBatchList.size);

                    batchList.Add(batchInfo);

                    inOrderBatchList.size = 0;
                    outOfOrderBatchList.size = 0;

                }

            }

            stencilsToPush.Dispose();
            stencilsToPop.Dispose();
            clipperIdList.Dispose();
            inOrderBatchList.Dispose();
            outOfOrderBatchList.Dispose();

        }

        public static bool ContainsInt(int test, ref List_Int32 clipRectList) {
            for (int i = 0; i < clipRectList.size; i++) {
                if (clipRectList.array[i] == test) return true;
            }

            return false;
        }

        public void UIForiaShapeBatch_OutOfOrderBatch(int batchBreakIndex, int activeMaterialId) {
            DrawInfo* drawInfoArray = drawList.GetArrayPointer();

            ref DrawInfo batchStart = ref drawInfoArray[inOrderBatchList.array[0]];
            ref StencilInfo batchStencilInfo = ref stencilList[batchStart.stencilIndex];

            for (int i = batchBreakIndex + 1; i < drawList.size; i++) {

                ref DrawInfo drawInfo = ref drawInfoArray[i];

                if (drawInfo.type != DrawType.Shape || drawInfo.materialId.index != activeMaterialId) {
                    continue;
                }

                bool containsClipRect = drawInfo.clipRectIndex == batchStart.clipRectIndex || ContainsInt(drawInfo.clipRectIndex, ref clipperIdList);
                bool requiresStencilOpen = false;

                if (!containsClipRect && clipperIdList.size + 1 >= k_MaxClipRects) {
                    continue;
                }

                if (batchStart.stencilIndex != drawInfo.stencilIndex) {
                    // may have to open a stencil 

                    ref StencilInfo stencilInfo = ref stencilList[drawInfo.stencilIndex];

                    if (batchStencilInfo.stencilDepth != stencilInfo.stencilDepth) {
                        continue;
                    }

                    // if stencil overlaps with any other stencil we marked to open, cannot batch.
                    if (!ContainsInt(drawInfo.stencilIndex, ref stencilsToPop)) {

                        // todo -- track failed stencils so we dont re-check

                        switch (stencilInfo.drawState) {

                            default:
                            case StencilSetupState.Uninitialized:
                                // if draw info overlaps anything between stencil start and self, cannot open stencil
                                // if stencil overlaps anything undrawn, cannot open
                                // if another unopened stencil is between this stencil and the draw info, cannot open here
                                // I think its best to aggressively open, opening the stencil and adding to batch are different actions

                                // // todo -- figure out clipper management, I think i want a different structure that allows for 1. compact data 2. easy processing. keep index into draw list for when we need to retrieve geometry info etc
                                // if (StencilsIntersect(stencilInfo.aabb, ref stencilsToPop)) {
                                //     continue;
                                // }

                                requiresStencilOpen = true;

                                break;

                            case StencilSetupState.Pushed:
                                // if there is an open stencil between the stencil and the drawInfo, cannot batch
                                // could mark passed stencil list to avoid re-checking
                            {
                                // for each stencil between drawInfo.stencilIndex and drawInfo.index
                                // if not closed/popped and intersects, cannot draw yet
                                for (int s = drawInfo.stencilIndex + 1; s < stencilList.size; s++) {
                                    if (stencilList[s].pushIndex > i) {
                                        break;
                                    }
                                }

                            }
                                break;

                            case StencilSetupState.Closed:
                                // error, should never hit this
                                break;

                            case StencilSetupState.Popped:
                                // error, should never hit this
                                break;

                        }
                    }

                }

                if (drawInfo.materialOverrideCount != batchStart.materialOverrideCount) {
                    continue;
                }

                if (drawInfo.materialOverrideValues != batchStart.materialOverrideValues) {
                    // note -- assumes they were already sorted!
                    if (UnsafeUtility.MemCmp(drawInfo.materialOverrideValues, batchStart.materialOverrideValues, sizeof(MaterialPropertyOverride) * batchStart.materialOverrideCount) != 0) {
                        continue;
                    }
                }

                if (!IntersectionTest(drawInfo.intersectedBounds, batchBreakIndex, i)) {
                    continue;
                }

                // if everything is ok up to this point we can add to the batch

                if (!containsClipRect) {
                    clipperIdList.array[clipperIdList.size++] = drawInfo.clipRectIndex;
                }

                if (requiresStencilOpen) {
                    stencilsToPop.Add(drawInfo.stencilIndex);
                    ref StencilInfo stencilInfo = ref stencilList[drawInfo.stencilIndex];
                    stencilInfo.drawState = StencilSetupState.Pushed;
                }

                outOfOrderBatchList.Add(i);
                drawInfo.flags |= DrawInfoFlags.BatchSet;

            }

        }

        private bool StencilsIntersect(in AxisAlignedBounds2D aabb, int idx, ref List_Int32 unopenedStencils) {

            StencilInfo* stencilArray = stencilList.GetArrayPointer();
            return false;

        }

        private bool IntersectionTest(in AxisAlignedBounds2D aabb, int orderedBatchEndIndex, int testIndex) {

            DrawInfo* drawInfoArray = drawList.GetArrayPointer();

            for (int i = orderedBatchEndIndex; i < testIndex; i++) {
                ref DrawInfo test = ref drawInfoArray[i];

                if ((test.flags & DrawInfoFlags.BatchSet) != 0) {
                    continue;
                }

                AxisAlignedBounds2D testBounds = test.intersectedBounds;

                // maybe can be done with simd bool4 by replacing && with &
                bool overlappingOrContains = aabb.xMax > testBounds.xMin && aabb.xMin < testBounds.xMax && aabb.yMax > testBounds.yMin && aabb.yMin < testBounds.yMax;

                if (overlappingOrContains) {
                    return false;
                }

            }

            return true;
        }

        public int UIForiaShapeBatch_InOrderBatch(int startIdx) {

            ProcessedDrawInfo* drawInfoArray = processedDrawList.GetArrayPointer();

            ref ProcessedDrawInfo batchStart = ref drawInfoArray[startIdx];
            int activeMaterialIndex = batchStart.materialId.index;
            inOrderBatchList.Add(batchStart.drawInfoIndex);
            clipperIdList[0] = batchStart.clipRectIndex;
            clipperIdList.size = 1;

            // open the required stencil for first item in batch if it isnt open yet
            // still dont know how to check ignored, maybe depth == 0? 
            // better not to have special cases where possible

            if (stencilList[batchStart.stencilIndex].drawState == StencilSetupState.Uninitialized) {
                stencilsToPush.Add(batchStart.stencilIndex);
                stencilList[batchStart.stencilIndex].drawState = StencilSetupState.Pushed;
            }

            int materialMaxClipRects = k_MaxClipRects; // todo -- implement this -> need a way to know where the clip rect index should go, in attribute or in single uniform

            int idx = batchStart.nextIndex;

            while (idx != -1) {
                ref ProcessedDrawInfo current = ref processedDrawList[idx];

                if (current.type != DrawType.Shape) {
                    return idx;
                }

                if (current.materialId.index != activeMaterialIndex) {
                    return idx;
                }

                // new stencil breaks in-order batching, we'll get around this in the out of order pass
                if (current.stencilIndex != batchStart.stencilIndex) {
                    return idx;
                }

                // todo -- check textures & material properties
                if (current.materialPropertySetId != batchStart.materialPropertySetId) {
                    return idx;
                }

                if (!ContainsInt(current.clipRectIndex, ref clipperIdList)) {

                    if (clipperIdList.size == materialMaxClipRects) {
                        return idx;
                    }

                    clipperIdList.array[clipperIdList.size++] = current.clipRectIndex;
                }

                inOrderBatchList.Add(current.drawInfoIndex);

                idx = current.nextIndex;
            }

            return processedDrawList.size;
        }

        private void IssueStencilUpdate() {

            if (stencilsToPop.size > 0) {

                for (int i = 0; i < stencilsToPop.size; i++) {

                    ref StencilInfo stencilInfo = ref stencilList[stencilsToPop[i]];
                    // setup a batch to close the stencil

                }

            }

            if (stencilsToPush.size > 0) {

                for (int i = 0; i < stencilsToPush.size; i++) {

                    ref StencilInfo stencilInfo = ref stencilList[stencilsToPush[i]];

                    // setup a batch to push the stencil
                    StencilState stencilState = new StencilState(true, 255, 255, CompareFunction.Equal, StencilOp.IncrementSaturate);

                    CreateStencilBatches(stencilInfo, stencilState);

                    Batch batch = new Batch {
                        stencilState = stencilState,
                        materialId = stencilInfo.materialId,
                        propertyOverrides = stencilInfo.propertyOverrides,
                        propertyOverrideCount = stencilInfo.propertyOverrideCount,
                        vertexLayout = stencilInfo.vertexLayout,
                        memberIdRange = new RangeInt(0, 1),
                        stencilRefValue = stencilInfo.refValue,
                        colorMask = stencilInfo.isHidden ? 0 : ColorWriteMask.All
                    };

                    renderCommands.Add(new RenderCommand() {
                        type = RenderCommandType.ElementBatch,
                        batchIndex = batchList.size
                    });

                    batchList.Add(batch);
                }

                // check if can batch with prev stencil (material & properties & vertex layout must match)
                // if it can, push it
                // if it cannot, begin new batch with same stencil info

            }

            stencilsToPush.size = 0;
            stencilsToPop.size = 0;

        }

        private void CreateStencilBatches(in StencilInfo stencilInfo, StencilState stencilState) {

            // need to store clipIndex per draw info somehow
            int idx = stencilInfo.drawInfoRange.start;
            int batchRangeStart = 0;

            while (idx < stencilInfo.drawInfoRange.end) {
                
                batchRangeStart = batchMemberList.size;
                batchMemberList.Add(idx);
                
                ref DrawInfo batchStart = ref drawList[idx++];
                clipperIdList.Add(batchStart.clipRectIndex);
                
                for (;idx < stencilInfo.drawInfoRange.end; idx++) {
                    ref DrawInfo current = ref drawList[idx];

                    if (!VertexLayout.Equal(batchStart.vertexLayout, current.vertexLayout) || !TryBatchClipRect(batchStart, current) || !TryBatchMaterial(batchStart, current)) {
                    
                        // if clipRect batched but material failed, leave it in buffer, better than figuring out if we should remove it or not
                        
                        batchList.Add(new Batch() {
                            stencilState = stencilState,
                            colorMask = ColorWriteMask.All,
                            materialId = batchStart.materialId,
                            propertyOverrides = batchStart.materialOverrideValues,
                            propertyOverrideCount = batchStart.materialOverrideCount,
                            stencilRefValue = stencilList[stencilInfo.parentIndex].refValue,
                            vertexLayout = batchStart.vertexLayout,
                            memberIdRange = new RangeInt(batchRangeStart, batchMemberList.size - batchRangeStart),
                        });
                        
                        break;

                    }
                    
                    batchMemberList.Add(idx);
                    
                }

            }
            
            ref DrawInfo lastBatchStart = ref drawList[batchMemberList[batchRangeStart]];

            batchList.Add(new Batch() {
                stencilState = stencilState,
                colorMask = ColorWriteMask.All,
                materialId = lastBatchStart.materialId,
                propertyOverrides = lastBatchStart.materialOverrideValues,
                propertyOverrideCount = lastBatchStart.materialOverrideCount,
                vertexLayout = lastBatchStart.vertexLayout,
                stencilRefValue = stencilList[stencilInfo.parentIndex].refValue,
                memberIdRange = new RangeInt(batchRangeStart, batchMemberList.size - batchRangeStart),
            });
            
        }

        private bool TryBatchMaterial(in DrawInfo batchStart, in DrawInfo current) {
            if (batchStart.materialId.index != current.materialId.index) {
                return false;
            }

            // todo -- known shaders should handle setting material overrides that are not textures to empty
            // or i guess treat texture overrides differently? depends on how sdf / text shaders end up with data and whether or not its what i expect
            // todo -- for known shaders try to use structured property batching if buffer is large enough
            // if (batchStart.materialId.index == MaterialId.UIForiaShape.index) { }

            // if shapeBuffer.size + material.requiredFloats
            // drawInfo.bufferOffset = shapebuffer.size;
            // shapeBuffer.size += material.requiredFloats;
            // else 
            // shapeBuffer = allocator.Allocate<float>(size);
            // shapeBuffer.size = 0;
            // renderCommands.Add(new RenderCommand() { type = SetShapeBuffer, data = shapeBuffer 
            return batchStart.propertySetId == current.propertySetId;

        }

        private bool TryBatchClipRect(in DrawInfo batchStart, in DrawInfo batchClipRectId) {
            // assume clip rect is inside shape data buffer
            if (batchStart.clipRectIndex == batchClipRectId.clipRectIndex) {
                return true;
            }

            if (batchStart.materialId.useClipBuffer != 0) {
                    
            }

            return true;

        }


    }

}