using UIForia.Layout;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Systems {

    [BurstCompile]
    internal struct ComputeOrientedBounds : IJob, IVertigoParallel {

        public DataList<ElementId>.Shared elementList;
        public ElementTable<ClipInfo> clipInfoTable;
        public ElementTable<float4x4> worldMatrices;
        public ElementTable<LayoutBoxInfo> layoutResultTable;
        public ParallelParams parallel { get; set; }

        public void Execute() {
            Run(0, elementList.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            for (int i = start; i < end; i++) {

                ElementId elementId = elementList[i];

                ref LayoutBoxInfo result = ref layoutResultTable[elementId];

                float width = result.actualSize.x;
                float height = result.actualSize.y;

                OrientedBounds orientedBounds = default;

                ref float4x4 worldMatrix = ref worldMatrices[elementId];

                float x = 0;
                float y = 0;

                orientedBounds.p0.x = worldMatrix.c0.x * x + worldMatrix.c1.x * y + worldMatrix.c3.x;
                orientedBounds.p0.y = (worldMatrix.c0.y * x + worldMatrix.c1.y * y + -worldMatrix.c3.y);

                orientedBounds.p1.x = worldMatrix.c0.x * width + worldMatrix.c1.x * y + worldMatrix.c3.x;
                orientedBounds.p1.y = (worldMatrix.c0.y * width + worldMatrix.c1.x * y + -worldMatrix.c3.y);

                orientedBounds.p2.x = worldMatrix.c0.x * width + worldMatrix.c1.x * height + worldMatrix.c3.x;
                orientedBounds.p2.y = (worldMatrix.c0.y * width + worldMatrix.c1.y * height + -worldMatrix.c3.y);

                orientedBounds.p3.x = worldMatrix.c0.x * x + worldMatrix.c1.x * height + worldMatrix.c3.x;
                orientedBounds.p3.y = (worldMatrix.c0.y * x + worldMatrix.c1.y * height + -worldMatrix.c3.y);

                clipInfoTable[elementId].orientedBounds = orientedBounds;

            }
        }

    }

}