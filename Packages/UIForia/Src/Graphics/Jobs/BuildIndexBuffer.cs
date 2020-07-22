using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct BuildIndexBuffer : IJob {

        public DataList<Batch>.Shared batchList;
        public DataList<int>.Shared batchMemberList;
        public DataList<MeshInfo> meshInfoList;
        public DataList<int>.Shared indexBuffer;
        public DataList<IndirectArg>.Shared indirectArgs;
        public DataList<int>.Shared combinedTriangleList;

        public void Execute() {

            int* trianglePtr = combinedTriangleList.GetArrayPointer();

            int offset = 0;
            
            for (int batchIdx = 0; batchIdx < batchList.size; batchIdx++) {

                ref Batch batch = ref batchList[batchIdx];

                if (batch.type == BatchType.Element || batch.type == BatchType.Text) {

                    int start = batch.memberIdRange.start;
                    int end = batch.memberIdRange.end;

                    int indexRangeStart = indexBuffer.size;

                    for (int i = start; i < end; i++) {
                        ref MeshInfo meshInfo = ref meshInfoList[batchMemberList[i]];

                        indexBuffer.AddRange(trianglePtr + meshInfo.triangleStart, meshInfo.triangleCount);

                    }

                    int indexCount = indexBuffer.size - indexRangeStart;

                    batch.indirectArgOffset = offset++;
                    
                    indirectArgs.Add(new IndirectArg() {
                        instanceCount = 1,
                        baseVertexLocation = 0,
                        startIndexLocation = indexRangeStart,
                        indexCountPerInstance = indexCount,
                        startInstanceLocation = 0
                    });
                    
                }

            }

        }

    }

}