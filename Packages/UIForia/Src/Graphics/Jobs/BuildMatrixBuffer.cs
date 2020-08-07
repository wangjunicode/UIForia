using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct BuildMatrixBuffer : IJob {

        // assumes this is already setup for proper size
        public List_Int32 matrixIndices;
        public DataList<float4x4>.Shared matrixBuffer;
        public DataList<DrawInfo2>.Shared drawList;

        public void Execute() {

            // could be persistent
            LongMap<int> ptrMap = new LongMap<int>(drawList.size, Allocator.Temp);

            DrawInfo2* drawArray = drawList.GetArrayPointer();
            for (int i = 0; i < drawList.size; i++) {
                ref DrawInfo2 drawInfo = ref drawArray[i];

                if (drawInfo.IsInternalDrawType()) { 

                    if (ptrMap.TryGetValue((long) drawInfo.matrix, out int idx)) {
                        matrixIndices.array[i] = idx;
                    }
                    else {
                        matrixIndices.array[i] = ptrMap.size;
                        ptrMap.Add((long) drawInfo.matrix, ptrMap.size);
                        matrixBuffer.Add(*drawInfo.matrix);
                    }

                }
                else {
                    matrixIndices.array[i] = 0;
                }

            }

            ptrMap.Dispose();

        }

    }

}