using UIForia.Graphics;
using UIForia.Rendering;
using UIForia.Util.Unsafe;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Systems {

    internal unsafe struct CallPainters_Managed : IJob, IVertigoParallelDeferred {

        public GCHandleArray<RenderBox> renderBoxTableHandle;
        public PerThreadObjectPool<RenderContext2> renderContextPool;
        public DataList<RenderCallInfo>.Shared renderCallList;
        public ElementTable<float4x4> matrices;

        [NativeSetThreadIndex]
        public int threadIndex;
        
        public ParallelParams.Deferred defer { get; set; }

        public void Execute() {
            Run(0, renderCallList.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            RenderBox[] renderBoxTable = renderBoxTableHandle.Get();
            RenderContext2 ctx = renderContextPool.GetForThread(threadIndex);
            
            for (int i = start; i < end; i++) {

                ElementId elementId = renderCallList[i].elementId;

                RenderBox box = renderBoxTable[elementId.index];

                if(box == null) continue;
                
                if (renderCallList[i].renderOp == 0) {
                    ctx.Setup(elementId, i, matrices.array + elementId.index);
                    box.PaintBackground2(ctx);
                }
                else {
                    ctx.Setup(elementId, i,  matrices.array + elementId.index);
                    box.PaintForeground2(ctx);
                }

            }
        }


    }

}