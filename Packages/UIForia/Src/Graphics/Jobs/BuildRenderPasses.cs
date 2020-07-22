using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

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
    internal unsafe struct BuildRenderPasses : IJob {

        public List_Int32 materialPermutationIds;

        public DataList<MaterialPermutation>.Shared materialPermutations;

        public DataList<RenderCommand>.Shared renderCommands;
        public DataList<DrawInfo2>.Shared drawList;
        public DataList<StencilInfo>.Shared stencilList;
        public DataList<AxisAlignedBounds2D>.Shared clipperBoundsList;
        public DataList<Batch>.Shared batchList;
        public DataList<int>.Shared batchMemberList;
        public DataList<RenderTraversalInfo> renderTraversalList;

        private BatchType currentBatchType;

        private List_Int32 stencilIdList;
        private List_Int32 inOrderBatchList;
        private List_Int32 outOfOrderBatchList;
        private List_Int32 stencilsToPush;
        private List_Int32 stencilsToPop;

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

        private void BuildOutOfOrderLinks(ref DataList<bool> requiresRender, ref DataList<DrawLink> drawLinks) {
            DrawLink dummy = default;

            ref DrawLink prevLink = ref dummy;

            int prevIdx = -1;

            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo2 drawInfo = ref drawList[i];

                requiresRender[i] = true;

                if (drawInfo.IsNonRendering()) {
                    continue;
                }

                ref RenderTraversalInfo renderTraversalInfo = ref renderTraversalList[i];

                if (!renderTraversalInfo.isStencilMember) {
                    drawLinks[i] = new DrawLink() {
                        prev = prevIdx,
                    };
                    prevLink.next = i;
                    prevLink = ref drawLinks[i];
                }

            }

            prevLink.next = -1;
        }

        public void Execute() {

            inOrderBatchList = new List_Int32(128, Allocator.Temp);
            outOfOrderBatchList = new List_Int32(128, Allocator.Temp);
            stencilsToPop = new List_Int32(16, Allocator.Temp);
            stencilsToPush = new List_Int32(16, Allocator.Temp);

            DataList<DrawLink> drawLinks = new DataList<DrawLink>(drawList.size, Allocator.Temp);

            DataList<bool> requiresRender = new DataList<bool>(drawList.size, Allocator.Temp);

            BuildOutOfOrderLinks(ref requiresRender, ref drawLinks);

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

                if (!requiresRender[i]) {
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

                        i = UIForiaTextBatch_InOrderBatch(ref requiresRender, i, out MaterialPermutation materialPermutation);

                        Batch batch = new Batch {
                            type = BatchType.Text,
                            materialPermutation = materialPermutation,
                            memberIdRange = new RangeInt(batchMemberOffset, inOrderBatchList.size),
                            // todo -- stencil setup
                        };

                        batchMemberList.AddRange(inOrderBatchList.array, inOrderBatchList.size);
                        inOrderBatchList.size = 0;

                        batchList.Add(batch);

                        renderCommands.Add(new RenderCommand() {
                            type = RenderCommandType.SDFTextBatch,
                            batchIndex = batchList.size - 1
                        });

                        break;

                    }

                    case DrawType2.UIForiaElement: {

                        int batchMemberOffset = batchMemberList.size;

                        i = UIForiaShapeBatch_InOrderBatch(ref requiresRender, i, out MaterialPermutation materialPermutation);

                        Batch batch = new Batch {
                            type = BatchType.Element,
                            materialPermutation = materialPermutation,
                            memberIdRange = new RangeInt(batchMemberOffset, inOrderBatchList.size),
                            // todo -- stencil setup
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

                    case DrawType2.PushStencilClip: {
                        break;
                    }

                    case DrawType2.PopClipper: {
                        // should know which clipper and its type?
                        break;
                    }

                }

            }

            stencilsToPush.Dispose();
            stencilsToPop.Dispose();
            inOrderBatchList.Dispose();
            outOfOrderBatchList.Dispose();
        }

        public int UIForiaTextBatch_InOrderBatch(ref DataList<bool> requiresRender, int startIdx, out MaterialPermutation permutation) {
            DrawInfo2* drawInfoArray = drawList.GetArrayPointer();

            inOrderBatchList.Add(startIdx);

            // open the required stencil for first item in batch if it isnt open yet
            // still dont know how to check ignored, maybe depth == 0? 
            // better not to have special cases where possible

            int stencilIndex = renderTraversalList[startIdx].stencilIndex;

            // if (stencilList[batchStart.stencilIndex].drawState == StencilSetupState.Uninitialized) {
            //     stencilsToPush.Add(batchStart.stencilIndex);
            //     stencilList[batchStart.stencilIndex].drawState = StencilSetupState.Pushed;
            // }

            TextMaterialSetup* materialSetup = (TextMaterialSetup*) drawInfoArray[startIdx].materialData;

            permutation = new MaterialPermutation {
                materialId = MaterialId.UIForiaShape,
                texture0 = materialSetup->faceTexture.textureId,
                texture1 = materialSetup->outlineTexture.textureId,
                texture2 = materialSetup->fontTextureId
            };

            for (int i = startIdx + 1; i < drawList.size; i++) {

                ref DrawInfo2 current = ref drawInfoArray[i];

                if (!requiresRender[i]) {
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

        public int UIForiaShapeBatch_InOrderBatch(ref DataList<bool> requiresRender, int startIdx, out MaterialPermutation permutation) {

            DrawInfo2* drawInfoArray = drawList.GetArrayPointer();

            inOrderBatchList.Add(startIdx);

            // open the required stencil for first item in batch if it isnt open yet
            // still dont know how to check ignored, maybe depth == 0? 
            // better not to have special cases where possible

            int stencilIndex = renderTraversalList[startIdx].stencilIndex;

            // if (stencilList[batchStart.stencilIndex].drawState == StencilSetupState.Uninitialized) {
            //     stencilsToPush.Add(batchStart.stencilIndex);
            //     stencilList[batchStart.stencilIndex].drawState = StencilSetupState.Pushed;
            // }

            ElementMaterialSetup* materialSetup = (ElementMaterialSetup*) drawInfoArray[startIdx].materialData;

            permutation = new MaterialPermutation {
                materialId = MaterialId.UIForiaShape,
                texture0 = materialSetup->bodyTexture.textureId,
                texture1 = materialSetup->outlineTexture.textureId
            };

            for (int i = startIdx + 1; i < drawList.size; i++) {

                ref DrawInfo2 current = ref drawInfoArray[i];

                if (!requiresRender[i]) {
                    continue;
                }

                if (current.drawType != DrawType2.UIForiaElement) {
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