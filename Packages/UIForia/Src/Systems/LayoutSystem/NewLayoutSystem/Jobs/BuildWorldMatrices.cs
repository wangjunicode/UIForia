using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Layout {

    [BurstCompile]
    internal unsafe struct BuildWorldMatrices : IJob {

        public ElementTable<float4x4> localMatrices;
        public ElementTable<float4x4> worldMatrices;

        public DataList<ElementId>.Shared elementList;
        public DataList<ElementId>.Shared parentList;

        public void Execute() {

            int isBurstActive = 1;
            CheckIsBurst(&isBurstActive);

            if (isBurstActive == 1) {

                for (int i = 0; i < elementList.size; i++) {

                    ElementId elementId = elementList[i];
                    ElementId parentId = parentList[i];

                    ref float4x4 localMatrix = ref localMatrices[elementId];
                    ref float4x4 parentWorldMatrix = ref worldMatrices[parentId];

                    worldMatrices[elementId] = math.mul(localMatrix, parentWorldMatrix);

                }
            }
            else {
                NonBurstExecute();
            }

        }

        // when not running in burst this is literally 10x faster. matrix multiply in managed is uncomfortably slow
        private void NonBurstExecute() {
            ElementId* elementListPtr = elementList.GetArrayPointer();
            ElementId* parentListPtr = parentList.GetArrayPointer();
            float4x4* loc = localMatrices.array;
            float4x4* wrld = worldMatrices.array;

            for (int i = 0; i < elementList.size; i++) {

                ElementId elementId = elementListPtr[i];
                ElementId parentId = parentListPtr[i];

                ref float4x4 left = ref loc[elementId.index];
                ref float4x4 right = ref wrld[parentId.index];

                float4x4 m = new float4x4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                );
                
                // this is an inlined 2d matrix multiplication. matrix multiply is stupidly slow in managed mode,
                // being 2d cuts down our work load by a factor of 6 according to the profiler
                m.c0.x = left.c0.x * right.c0.x + left.c1.x * right.c0.y;
                m.c0.y = left.c0.y * right.c0.x + left.c1.y * right.c0.y;
                m.c1.x = left.c0.x * right.c1.x + left.c1.x * right.c1.y;
                m.c1.y = left.c0.y * right.c1.x + left.c1.y * right.c1.y;
                m.c3.x = left.c0.x * right.c3.x + left.c1.x * right.c3.y + left.c3.x;
                m.c3.y = left.c0.y * right.c3.x + left.c1.y * right.c3.y + left.c3.y;

                wrld[elementId.index] = m;

            }
        }

        [BurstDiscard]
        private static void CheckIsBurst(int* arg) {
            *arg = 0;
        }

    }

}