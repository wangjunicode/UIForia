using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia.Graphics {

    public unsafe struct MaterialPermutation {

        public MaterialId materialId;

        public int overrideCount;
        public MaterialPropertyOverride* overrides;

        public int texture0;
        public int texture1;
        public int texture2;

    }

    [BurstCompile]
    internal unsafe struct CreateRenderPasses : IJob {

        public List_Int32 materialPermutationIds;

        public DataList<MaterialPermutation>.Shared materialPermutations;

        public DataList<RenderCommand>.Shared renderCommands;
        public DataList<DrawInfo2>.Shared drawList;
        public DataList<StencilInfo>.Shared stencilList;
        public DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;
        public DataList<Batch>.Shared batchList;
        public DataList<int>.Shared batchMemberList;
        public DataList<RenderTraversalInfo> renderTraversalList;
        public List_Int32 initialCandidateList;
        public List_Int32 candidateBuffer;

        private BatchType currentBatchType;

        private List_Int32 stencilIdList;
        private List_Int32 inOrderBatchList;
        private List_Int32 outOfOrderBatchList;
        private List_Int32 stencilsToPush;
        private List_Int32 stencilsToPop;
        public DataList<AxisAlignedBounds2D> transformedBounds;

        // todo -- this should work to optimize out of order searches
        public void FindNextWithCompatibleMaterial(ref DataList<bool> requiresRender) {
            IntMap<int> map = new IntMap<int>(64, Allocator.Temp);

            int lastElementMaterialUseId;

            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo2 drawInfo = ref drawList[i];

                if (!requiresRender[i]) {
                    continue;
                }

                if (drawInfo.IsNonRendering()) {
                    continue;
                }

                // if is element & untextured, can batch with either last permutation that was textured or current permutation
                // only applies while not in a stencil range

                int idx = 0;
                if (map.TryGetReference(materialPermutationIds[i], ref idx)) { }

            }

            map.Dispose();
        }

        public void Execute() {
            initialCandidateList = new List_Int32(32, Allocator.Temp);
            candidateBuffer = new List_Int32(32, Allocator.Temp);
            inOrderBatchList = new List_Int32(128, Allocator.Temp);
            outOfOrderBatchList = new List_Int32(128, Allocator.Temp);
            stencilsToPop = new List_Int32(16, Allocator.Temp);
            stencilsToPush = new List_Int32(16, Allocator.Temp);

            //  DataList<DrawLink> drawLinks = new DataList<DrawLink>(drawList.size, Allocator.Temp);

            //      DataList<RenderData> renderDataList = new DataList<RenderData>(drawList.size, Allocator.Temp);

            //   BuildOutOfOrderLinks(ref renderDataList, ref drawLinks);

            // for each open stencil
            // if in order pointer >= pop index
            // move to closed list

            // in order just walks draw list 
            // out of order uses draw links starting at last idx of in order batch + 1
            // will want to keep a ptr to 'next-with-material-permutation' for better skipping

            // pass over stencils and clippers and see if a clipper is culled
            // if clipper is culled, any element using that clipper is also culled
            // if parent clipper is culled, then so is that child clipper

            for (int i = 0; i < drawList.size; i++) {

                // we want to skip to the first thing that actually draws
                // if we hit any stenciling in here we ignore it
                // we do need to push callbacks though

                ref DrawInfo2 drawInfo = ref drawList[i];
                ref RenderTraversalInfo renderInfo = ref renderTraversalList[i];

                if (!renderInfo.requiresRendering) {
                    continue;
                }

                switch (drawInfo.drawType) {

                    case DrawType2.Callback:
                        // can track indices of commands and limit ooob search range between them
                        renderCommands.Add(new RenderCommand() {
                            type = RenderCommandType.Callback,
                            data = drawInfo.shapeData
                        });
                        break;

                    case DrawType2.UIForiaText: {

                        int batchMemberOffset = batchMemberList.size;

                        i = UIForiaTextBatch_InOrderBatch(ref renderTraversalList, i, out MaterialPermutation materialPermutation) - 1;

                        UIForiaTextBatch_OutOfOrderBatch(ref renderTraversalList, i + 1, ref materialPermutation);

                        SubmitInorderBatch(renderInfo, materialPermutation, batchMemberOffset, BatchType.Text, RenderCommandType.SDFTextBatch);

                        break;

                    }

                    case DrawType2.UIForiaElement: {

                        int batchMemberOffset = batchMemberList.size;

                        i = UIForiaShapeBatch_InOrderBatch(ref renderTraversalList, i, out MaterialPermutation materialPermutation) - 1;

                        UIForiaShapeBatch_OutOfOrderBatch(ref renderTraversalList, i + 1, ref materialPermutation);

                        SubmitInorderBatch(renderInfo, materialPermutation, batchMemberOffset, BatchType.Element, RenderCommandType.ElementBatch);

                        break;
                    }

                }

            }

            stencilsToPush.Dispose();
            stencilsToPop.Dispose();
            inOrderBatchList.Dispose();
            outOfOrderBatchList.Dispose();
            initialCandidateList.Dispose();
        }

        private void UIForiaShapeBatch_OutOfOrderBatch(ref DataList<RenderTraversalInfo> renderDataList, int startIdx, ref MaterialPermutation permutation) {
            DrawInfo2* drawInfoArray = drawList.GetArrayPointer();
            RenderTraversalInfo* renderInfoArray = renderDataList.GetArrayPointer();

            // open the required stencil for first item in batch if it isnt open yet
            // still dont know how to check ignored, maybe depth == 0? 
            // better not to have special cases where possible

            int stencilIndex = renderTraversalList[startIdx].stencilIndex;

            // i can assign a stencil id to all elements up front
            // i know the stencil depth and bounds
            // i know the stencil setup state
            // using that i can figure out where batching can happen

            // can optimize out of order searches pretty easily later on

            if (stencilList[stencilIndex].drawState == StencilSetupState.Uninitialized) {
                stencilsToPush.Add(stencilIndex);
                stencilList[stencilIndex].drawState = StencilSetupState.Pushed;
            }

            // gather candidates then cull?
            // or step by step 
            // gather is less work at a time, likely better

            bool isStencilMember = renderTraversalList[startIdx].isStencilMember;

            // startIdx + 1 because startIdx broke the batch

            // find other elements that have yet to be rendered
            // if we encounter a batch breaking command
            //    unknown material
            //    callback
            //    render target change
            //    clear?
            //    etc
            // stop.

            initialCandidateList.size = 0;

            for (int i = startIdx + 1; i < drawList.size; i++) {

                ref DrawInfo2 current = ref drawInfoArray[i];
                ref RenderTraversalInfo renderInfo = ref renderInfoArray[i];

                // todo -- break for other types also
                if (current.drawType == DrawType2.Callback) {
                    break;
                }

                if (current.drawType == DrawType2.UIForiaElement) {
                    if (renderInfo.requiresRendering && !renderInfo.isStencilMember) {
                        initialCandidateList.Add(i);
                    }
                }
            }

            if (initialCandidateList.size == 0) {
                return;
            }

            candidateBuffer.size = 0;
            AxisAlignedBounds2D* transformedBoundsArray = transformedBounds.GetArrayPointer();

            for (int i = 0; i < initialCandidateList.size; i++) {

                // discard the ones that are in different stencils for now, improve this later

                int idx = initialCandidateList.array[i];

                ref DrawInfo2 current = ref drawInfoArray[idx];
                ref AxisAlignedBounds2D currentBounds = ref transformedBoundsArray[idx];

                bool fitsInBatch = true;

                for (int j = startIdx; j < idx; j++) {

                    ref RenderTraversalInfo renderInfo = ref renderInfoArray[j];
                    ref DrawInfo2 test = ref drawInfoArray[j];

                    if (!renderInfo.requiresRendering || (test.drawType & (DrawType2.PushClipRect | DrawType2.PopClipRect)) != 0) {
                        continue;
                    }

                    // make sure the screen bounds do not overlap

                    ref AxisAlignedBounds2D testBounds = ref transformedBoundsArray[j];
                    if (testBounds.xMax >= currentBounds.xMin && testBounds.xMin <= currentBounds.xMax && testBounds.yMax >= currentBounds.yMin && testBounds.yMin <= currentBounds.yMax) {
                        fitsInBatch = false;
                        break;
                    }
                }

                if (!fitsInBatch) continue;

                // test material match, if it fits mark as rendered and add to batch list
                ElementMaterialSetup* materialSetup = (ElementMaterialSetup*) current.materialData;

                int bodyTextureId = materialSetup->bodyTexture.textureId;
                int outlineTextureId = materialSetup->outlineTexture.textureId;

                bool passTexture0 = permutation.texture0 == 0 || permutation.texture0 == bodyTextureId || bodyTextureId == 0;
                bool passTexture1 = permutation.texture1 == 0 || permutation.texture1 == outlineTextureId || outlineTextureId == 0;

                if (passTexture0 && passTexture1) {

                    if (permutation.texture0 == 0) {
                        permutation.texture0 = bodyTextureId;
                    }

                    if (permutation.texture1 == 0) {
                        permutation.texture1 = outlineTextureId;
                    }

                    renderInfoArray[idx].requiresRendering = false;
                    inOrderBatchList.Add(idx);
                }

            }

        }

        private void UIForiaTextBatch_OutOfOrderBatch(ref DataList<RenderTraversalInfo> renderDataList, int startIdx, ref MaterialPermutation permutation) {
            DrawInfo2* drawInfoArray = drawList.GetArrayPointer();
            RenderTraversalInfo* renderInfoArray = renderDataList.GetArrayPointer();

            // todo -- need to ignore stencils for now
            int stencilIndex = renderTraversalList[startIdx].stencilIndex;

            if (stencilList[stencilIndex].drawState == StencilSetupState.Uninitialized) {
                stencilsToPush.Add(stencilIndex);
                stencilList[stencilIndex].drawState = StencilSetupState.Pushed;
            }

            // gather candidates then cull?
            // or step by step 
            // gather is less work at a time, likely better

            bool isStencilMember = renderTraversalList[startIdx].isStencilMember;

            // startIdx + 1 because startIdx broke the batch

            // find other elements that have yet to be rendered
            // if we encounter a batch breaking command
            //    unknown material
            //    callback
            //    render target change
            //    clear?
            //    etc
            // stop.

            initialCandidateList.size = 0;

            for (int i = startIdx + 1; i < drawList.size; i++) {

                ref DrawInfo2 current = ref drawInfoArray[i];
                ref RenderTraversalInfo renderInfo = ref renderInfoArray[i];

                // todo -- break for other types also
                if (current.drawType == DrawType2.Callback) {
                    break;
                }

                if (current.drawType == DrawType2.UIForiaText) {
                    if (renderInfo.requiresRendering && !renderInfo.isStencilMember) {
                        initialCandidateList.Add(i);
                    }
                }
            }

            if (initialCandidateList.size == 0) {
                return;
            }

            candidateBuffer.size = 0;
            AxisAlignedBounds2D* transformedBoundsArray = transformedBounds.GetArrayPointer();

            for (int i = 0; i < initialCandidateList.size; i++) {

                // discard the ones that are in different stencils for now, improve this later

                int idx = initialCandidateList.array[i];

                ref DrawInfo2 current = ref drawInfoArray[idx];
                ref AxisAlignedBounds2D currentBounds = ref transformedBoundsArray[idx];

                bool fitsInBatch = true;

                for (int j = startIdx; j < idx; j++) {

                    ref RenderTraversalInfo renderInfo = ref renderInfoArray[j];
                    ref DrawInfo2 test = ref drawInfoArray[j];

                    if (!renderInfo.requiresRendering || (test.drawType & (DrawType2.PushClipRect | DrawType2.PopClipRect)) != 0) {
                        continue;
                    }

                    // make sure the screen bounds do not overlap

                    ref AxisAlignedBounds2D testBounds = ref transformedBoundsArray[j];
                    if (testBounds.xMax >= currentBounds.xMin && testBounds.xMin <= currentBounds.xMax && testBounds.yMax >= currentBounds.yMin && testBounds.yMin <= currentBounds.yMax) {
                        fitsInBatch = false;
                        break;
                    }
                }

                if (!fitsInBatch) continue;

                // test material match, if it fits mark as rendered and add to batch list
                TextMaterialSetup* materialSetup = (TextMaterialSetup*) current.materialData;

                if (materialSetup->fontTextureId != permutation.texture2) {
                    continue;
                }

                int faceTextureId = materialSetup->faceTexture.textureId;
                int outlineTextureId = materialSetup->outlineTexture.textureId;

                bool passTexture0 = permutation.texture0 == 0 || permutation.texture0 == faceTextureId || faceTextureId == 0;
                bool passTexture1 = permutation.texture1 == 0 || permutation.texture1 == outlineTextureId || outlineTextureId == 0;

                if (passTexture0 && passTexture1) {

                    if (permutation.texture0 == 0) {
                        permutation.texture0 = faceTextureId;
                    }

                    if (permutation.texture1 == 0) {
                        permutation.texture1 = outlineTextureId;
                    }

                    renderInfoArray[idx].requiresRendering = false;
                    inOrderBatchList.Add(idx);
                }

            }

        }

        private void SubmitInorderBatch(RenderTraversalInfo renderInfo, MaterialPermutation materialPermutation, int batchMemberOffset, BatchType batchType, RenderCommandType cmdType) {
            ref StencilInfo stencilInfo = ref stencilList[renderInfo.stencilIndex];

            Batch batch = new Batch {
                type = batchType,
                materialPermutation = materialPermutation,
                memberIdRange = new RangeInt(batchMemberOffset, inOrderBatchList.size),
                stencilType = stencilInfo.stencilDepth == 0 ? StencilType.Ignore : StencilType.Draw,
                stencilRefValue = (byte) stencilInfo.stencilDepth,
                colorMask = ColorWriteMask.All
            };

            batchMemberList.AddRange(inOrderBatchList.array, inOrderBatchList.size);
            inOrderBatchList.size = 0;

            if (stencilsToPop.size > 0) { }

            if (stencilsToPush.size > 0) {
                PushStencils();
            }

            // need to add this after stencil, but can build it before stencils
            batchList.Add(batch);
            renderCommands.Add(new RenderCommand() {
                type = cmdType,
                batchIndex = batchList.size - 1
            });
        }

        private void PushStencils() {
            for (int i = 0; i < stencilsToPush.size; i++) {
                ref StencilInfo stencilInfo = ref stencilList[stencilsToPush.array[i]];

                for (int j = stencilInfo.beginIndex; j < stencilInfo.pushIndex; j++) {
                    ref DrawInfo2 drawInfo = ref drawList[j];
                    switch (drawInfo.drawType) {
                        case DrawType2.UIForiaElement:

                            int batchMemberOffset = batchMemberList.size;

                            j = UIForiaShapeBatch_InOrderBatch(ref renderTraversalList, j, out MaterialPermutation materialPermutation) - 1;

                            Batch batch = new Batch() {
                                type = BatchType.Element,
                                stencilType = StencilType.Push,
                                materialPermutation = materialPermutation,
                                memberIdRange = new RangeInt(batchMemberOffset, inOrderBatchList.size),
                                stencilRefValue = (byte) (stencilInfo.stencilDepth - 1),
                                colorMask = ColorWriteMask.All,
                            };

                            batchMemberList.AddRange(inOrderBatchList.array, inOrderBatchList.size);
                            inOrderBatchList.size = 0;
                            batchList.Add(batch);
                            renderCommands.Add(new RenderCommand() {
                                type = RenderCommandType.ElementBatch,
                                batchIndex = batchList.size - 1
                            });
                            break;
                    }
                }

            }

            stencilsToPush.size = 0;

        }

        public int UIForiaTextBatch_InOrderBatch(ref DataList<RenderTraversalInfo> renderDataList, int startIdx, out MaterialPermutation permutation) {
            DrawInfo2* drawInfoArray = drawList.GetArrayPointer();
            RenderTraversalInfo* renderInfoArray = renderDataList.GetArrayPointer();

            inOrderBatchList.Add(startIdx);

            // open the required stencil for first item in batch if it isnt open yet
            // still dont know how to check ignored, maybe depth == 0? 
            // better not to have special cases where possible

            int stencilIndex = renderTraversalList[startIdx].stencilIndex;

            if (stencilList[stencilIndex].drawState == StencilSetupState.Uninitialized) {
                stencilsToPush.Add(stencilIndex);
                stencilList[stencilIndex].drawState = StencilSetupState.Pushed;
            }

            TextMaterialSetup* materialSetup = (TextMaterialSetup*) drawInfoArray[startIdx].materialData;

            permutation = new MaterialPermutation {
                materialId = MaterialId.UIForiaShape,
                texture0 = materialSetup->faceTexture.textureId,
                texture1 = materialSetup->outlineTexture.textureId,
                texture2 = materialSetup->fontTextureId
            };

            bool isStencilMember = renderTraversalList[startIdx].isStencilMember;

            for (int i = startIdx + 1; i < drawList.size; i++) {

                ref DrawInfo2 current = ref drawInfoArray[i];
                ref RenderTraversalInfo renderInfo = ref renderInfoArray[i];

                if (renderInfo.isStencilMember != isStencilMember || !renderInfo.requiresRendering || (current.drawType & (DrawType2.PushClipRect | DrawType2.PopClipRect)) != 0) {
                    continue;
                }

                if (current.drawType != DrawType2.UIForiaText) {
                    return i;
                }

                // new stencil breaks in-order batching, we'll get around this in the out of order pass
                if (renderTraversalList[i].stencilIndex != stencilIndex) {
                    // if stencil depths are the same
                    // check open states
                    // if can open and not intersecting then add to stencils to open list
                    // otherwise break batch
                    return i;
                }

                TextMaterialSetup* setup = (TextMaterialSetup*) current.materialData;

                // i need to see if the font texture is different
                if (setup->fontTextureId != permutation.texture2) {
                    return i;
                }

                // if no textures are set for the batch, set it this element's

                int prevTex0 = permutation.texture0;
                int prevTex1 = permutation.texture1;

                if (permutation.texture0 == 0) {
                    permutation.texture0 = setup->faceTexture.textureId;
                }

                if (permutation.texture1 == 0) {
                    permutation.texture1 = setup->outlineTexture.textureId;
                }

                // when one texture passes and the other doesn't, we need to undo our change

                if (setup->faceTexture.textureId != 0 && permutation.texture0 != setup->faceTexture.textureId) {
                    permutation.texture0 = prevTex0;
                    permutation.texture1 = prevTex1;
                    return i;
                }

                if (setup->outlineTexture.textureId != 0 && permutation.texture1 != setup->outlineTexture.textureId) {
                    permutation.texture0 = prevTex0;
                    permutation.texture1 = prevTex1;
                    return i;
                }

                inOrderBatchList.Add(i);

            }

            return drawList.size;
        }

        public int UIForiaShapeBatch_InOrderBatch(ref DataList<RenderTraversalInfo> renderDataList, int startIdx, out MaterialPermutation permutation) {

            DrawInfo2* drawInfoArray = drawList.GetArrayPointer();
            RenderTraversalInfo* renderInfoArray = renderDataList.GetArrayPointer();

            inOrderBatchList.Add(startIdx);

            // open the required stencil for first item in batch if it isnt open yet
            // still dont know how to check ignored, maybe depth == 0? 
            // better not to have special cases where possible

            int stencilIndex = renderTraversalList[startIdx].stencilIndex;

            // i can assign a stencil id to all elements up front
            // i know the stencil depth and bounds
            // i know the stencil setup state
            // using that i can figure out where batching can happen

            // can optimize out of order searches pretty easily later on

            if (stencilList[stencilIndex].drawState == StencilSetupState.Uninitialized) {
                stencilsToPush.Add(stencilIndex);
                stencilList[stencilIndex].drawState = StencilSetupState.Pushed;
            }

            ElementMaterialSetup* materialSetup = (ElementMaterialSetup*) drawInfoArray[startIdx].materialData;

            permutation = new MaterialPermutation {
                materialId = MaterialId.UIForiaShape,
                texture0 = materialSetup->bodyTexture.textureId,
                texture1 = materialSetup->outlineTexture.textureId
            };

            bool isStencilMember = renderTraversalList[startIdx].isStencilMember;
            for (int i = startIdx + 1; i < drawList.size; i++) {

                ref DrawInfo2 current = ref drawInfoArray[i];
                ref RenderTraversalInfo renderInfo = ref renderInfoArray[i];

                if (renderInfo.isStencilMember != isStencilMember || !renderInfo.requiresRendering || (current.drawType & (DrawType2.PushClipRect | DrawType2.PopClipRect)) != 0) {
                    continue;
                }

                if (current.drawType != DrawType2.UIForiaElement) {
                    return i;
                }

                // new stencil breaks in-order batching, we'll get around this in the out of order pass
                if (renderInfo.stencilIndex != stencilIndex) {
                    // if stencil depths are the same
                    // check open states
                    // if can open and not intersecting then add to stencils to open list
                    // otherwise break batch
                    return i;
                }

                ElementMaterialSetup* setup = (ElementMaterialSetup*) current.materialData;

                // if no textures are set for the batch, set it this element's

                int prevTex0 = permutation.texture0;
                int prevTex1 = permutation.texture1;

                if (permutation.texture0 == 0) {
                    permutation.texture0 = setup->bodyTexture.textureId;
                }

                if (permutation.texture1 == 0) {
                    permutation.texture1 = setup->outlineTexture.textureId;
                }

                // when one texture passes and the other doesn't, we need to undo our change

                if (setup->bodyTexture.textureId != 0 && permutation.texture0 != setup->bodyTexture.textureId) {
                    permutation.texture0 = prevTex0;
                    permutation.texture1 = prevTex1;
                    return i;
                }

                if (setup->outlineTexture.textureId != 0 && permutation.texture1 != setup->outlineTexture.textureId) {
                    permutation.texture0 = prevTex0;
                    permutation.texture1 = prevTex1;
                    return i;
                }

                inOrderBatchList.Add(i);

            }

            return drawList.size;

        }

    }

}