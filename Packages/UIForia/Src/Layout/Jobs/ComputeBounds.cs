using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Layout {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct ComputeBounds : IJob, IUIForiaParallel {

        public CheckedArray<Size> sizes;
        public CheckedArray<OrientedBounds> bounds;
        public CheckedArray<float4x4> worldMatrices;

        public ParallelParams parallel { get; set; }

        public void Execute() {
            Run(0, sizes.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        private void Run(int start, int end) {

            for (int i = start; i < end; i++) {

                Size size = sizes[i];

                ref float4x4 worldMatrix = ref worldMatrices.array[i];
                ref OrientedBounds orientedBounds = ref bounds.array[i];

                const float x = 0;
                const float y = 0;

                orientedBounds.p0.x = worldMatrix.c0.x * x + worldMatrix.c1.x * y + worldMatrix.c3.x;
                orientedBounds.p0.y = (worldMatrix.c0.y * x + worldMatrix.c1.y * y + -worldMatrix.c3.y);

                orientedBounds.p1.x = worldMatrix.c0.x * size.width + worldMatrix.c1.x * y + worldMatrix.c3.x;
                orientedBounds.p1.y = (worldMatrix.c0.y * size.width + worldMatrix.c1.x * y + -worldMatrix.c3.y);

                orientedBounds.p2.x = worldMatrix.c0.x * size.width + worldMatrix.c1.x * size.height + worldMatrix.c3.x;
                orientedBounds.p2.y = (worldMatrix.c0.y * size.width + worldMatrix.c1.y * size.height + -worldMatrix.c3.y);

                orientedBounds.p3.x = worldMatrix.c0.x * x + worldMatrix.c1.x * size.height + worldMatrix.c3.x;
                orientedBounds.p3.y = (worldMatrix.c0.y * x + worldMatrix.c1.y * size.height + -worldMatrix.c3.y);

            }
        }

    }

}