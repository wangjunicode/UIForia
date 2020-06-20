using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Graphics {

    public enum RenderCommandType {

        Batch

    }

    public unsafe struct RenderCommand {

        public RenderCommandType type;
        public int batchIndex;
        public int meshIndex;
        public int renderTargetId;
        public byte* data; // context dependent. for batches this is float4 list for clipping 

    }

    [BurstCompile]
    internal struct TransparentRenderPassJob : IJob {

        public DataList<DrawInfo>.Shared drawList;
        public DataList<Batch>.Shared batchList;
        public DataList<int>.Shared batchMemberList;
        public DataList<RenderCommand>.Shared renderCommands;

        public enum BatchType {

            None,
            Text,
            Shape,
            Mesh

        }

        private BatchType currentBatchType;
        private int currentBatchId;
        private int nextBatchId;
        private int activeMaterialId;
        private RangeInt currentBatchRange;
        private VertexLayout currentVertexLayout;
        
        public void Execute() {

            currentBatchRange = default;

            // profile pre-locating the batch boundaries, might be better with less branching

            for (int i = 0; i < drawList.size; i++) {

                ref DrawInfo drawInfo = ref drawList[i];

                if (drawInfo.batchId != 0) continue;

                if ((drawInfo.type & DrawType.Shape) != 0) {

                    if (currentBatchType == BatchType.None) {
                        BeginShapeBatch(i, ref drawInfo);
                    }
                    else if (currentBatchType == BatchType.Shape) {
                        TryAddToShapeBatch(i, ref drawInfo);
                    }
                    else if (currentBatchType == BatchType.Mesh) {
                        EndBatch();
                        BeginShapeBatch(i, ref drawInfo);
                    }
                    else if (currentBatchType == BatchType.Text) {
                        EndBatch();
                        BeginShapeBatch(i, ref drawInfo);
                    }

                }
            }

            EndBatch();

        }

        // configure this with a max search limit
        private void OutOfOrderBatch() {
            // search forward until we hit a render state change
            // if state change is clip rect
            //    if we have space for more clip rects
            //    add it, keep going
            // if state change is clip shape
            //    see if clip shape is in current atlas
            //    if it is, keep going
            //    if it isn't
            //        see if we can add it
            //        if we can, enqueue it, keep going
            //    if we can't, stop.
            // if state change is render target, stop
            // if next info is draw command
            //     if unknown material, stop, end batch
            //     if known material
            //         check all overlaps (they should be being buffered as we go)
            //        if no overlaps, add to batch
            // 

            // if we hit something we add the batch, set it's batch id so it gets skipped later

        }

        private void EndBatch() {
            switch (currentBatchType) {

                case BatchType.Text:
                    break;

                case BatchType.Shape: {

                    OutOfOrderBatch();

                    if (currentBatchRange.length == 0) {
                        return;
                    }

                    renderCommands.Add(new RenderCommand() {
                        type = RenderCommandType.Batch, // maybe mesh batch or whatever
                        batchIndex = batchList.size,
                    });

                    batchList.Add(new Batch() {
                        vertexLayout = currentVertexLayout,
                        memberIdRange = currentBatchRange
                    });

                    break;
                }

                case BatchType.Mesh:
                    break;

                case BatchType.None:
                    break;

            }

            currentBatchType = BatchType.None;

            // if ending a shape batch,

        }

        private void TryAddToShapeBatch(int idx, ref DrawInfo drawInfo) {

            if (!VertexLayout.Equal(drawInfo.vertexLayout, currentVertexLayout)) {
                EndBatch();
                BeginShapeBatch(idx, ref drawInfo);
                return;
            }
            
            currentBatchRange.length++;
            batchMemberList.Add(idx);
            
            // if vertex layout is different, end batch
            // if material id is different, EndBatch()
            // if material id is the same but has property range set, EndBatch()
            // if material id is the same but texture range set, see if we can atlas the texture
            //    if we can, add to batch, inject texture atlas call
            //    if we cannot, EndBatch()

        }

        private void BeginShapeBatch(int idx, ref DrawInfo drawInfo) {
            currentBatchRange.start = batchMemberList.size;
            currentBatchRange.length = 1;
            currentBatchType = BatchType.Shape;
            currentVertexLayout = drawInfo.vertexLayout;
            activeMaterialId = drawInfo.materialKeyIndex; // todo -- wrong
            batchMemberList.Add(idx);
        }

        private bool IntersectionTest(in DrawInfo drawInfo, in DataList<DrawInfo> buffer, int orderedBatchEndIndex, int testIndex, ElementTable<ClipInfo> clipInfoTable) {
            return true;
        }

        private bool IsKnownMaterial(MaterialId candidateMaterialId) {
            return true;
        }

    }

}