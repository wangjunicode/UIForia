using UIForia.Graphics;
using UIForia.Graphics.ShapeKit;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Systems {

    internal unsafe struct CallPainters_Managed : IJob, IVertigoParallel {

        public GCHandleArray<RenderBox> renderBoxTableHandle;
        public PerThreadObjectPool<RenderContext2> renderContextPool;
        public DataList<RenderCallInfo>.Shared renderCallList;
        public ElementTable<float4x4> matrices;
        public ElementTable<ClipInfo> clipInfoTable;
        public DataList<OverflowBounds>.Shared clipperBoundsList;

        [NativeSetThreadIndex] public int threadIndex;

        public ParallelParams parallel { get; set; }

        public void Execute() {
            Run(0, renderCallList.size);
        }

        public void Execute(int start, int count) {
            Run(start, start + count);
        }

        private void Run(int start, int end) {

            RenderBox[] renderBoxTable = renderBoxTableHandle.Get();
            RenderContext2 ctx = renderContextPool.GetForThread(threadIndex);

            for (int i = start; i < end; i++) {

                ElementId elementId = renderCallList[i].elementId;

                RenderBox box = renderBoxTable[elementId.index];

                if (box == null) continue;

                MaterialId materialId = box.materialId;

                OverflowBounds* overflowBounds = default;
                int clipperIndex = clipInfoTable[elementId].clipperIndex;

                if (clipperIndex >= 2) {
                    overflowBounds = clipperBoundsList.GetArrayPointer() + clipperIndex;
                }

                int drawIdx = ctx.drawList.size;
                if (renderCallList[i].renderOp == 0) {
                    ctx.Setup(materialId, overflowBounds, i, matrices.array + elementId.index);
                    box.PaintBackground2(ctx);
                    box.bgRenderContext = ctx;
                    box.bgRenderRange = new RangeInt(drawIdx, ctx.drawList.size);
                }
                else {
                    ctx.Setup(materialId, overflowBounds, i, matrices.array + elementId.index);
                    box.PaintForeground2(ctx);
                    box.fgRenderContext = ctx;
                    box.fgRenderRange = new RangeInt(drawIdx, ctx.drawList.size);
                }

            }
        }

    }

}