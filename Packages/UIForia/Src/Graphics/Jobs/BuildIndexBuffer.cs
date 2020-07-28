using UIForia.Util;
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

        public void Execute() {
            int* batchMemberArray = batchMemberList.GetArrayPointer();
            
            int triangleCount = 0;
            for (int batchIdx = 0; batchIdx < batchList.size; batchIdx++) {
                ref Batch batch = ref batchList[batchIdx];

                if (batch.type == BatchType.Text || batch.type == BatchType.Element) {
                    
                    int start = batch.memberIdRange.start;
                    int end = batch.memberIdRange.end;
                    
                    for (int i = start; i < end; i++) {
                        ref MeshInfo meshInfo = ref meshInfoList[batchMemberArray[i]];
                        triangleCount += meshInfo.vertexCount * 6;
                    }
                }

            }

            indexBuffer.EnsureCapacity(triangleCount);
            indexBuffer.size = triangleCount;

            int* triangles = indexBuffer.GetArrayPointer();
            int triangleIndex = 0;

            int vertexOffset = 0;
            
            for (int batchIdx = 0; batchIdx < batchList.size; batchIdx++) {

                ref Batch batch = ref batchList[batchIdx];

                // I guess this includes text decorations, I'm not sure how to handle those since they arent really quads.
                // I can maybe repurpose some data in order to get the uvs to stretch correctly 

                int cidx1 = 1 << 24;
                int cidx2 = 2 << 24;
                int cidx3 = 3 << 24;
                
                if (batch.type == BatchType.Element || batch.type == BatchType.Text) {
                    
                    int start = batch.memberIdRange.start;
                    int end = batch.memberIdRange.end;
                    int indexRangeStart = triangleIndex;

                    for (int i = start; i < end; i++) {
                    
                        ref MeshInfo meshInfo = ref meshInfoList[batchMemberArray[i]];

                        for (int j = 0; j < meshInfo.vertexCount; j++) {
                            triangles[triangleIndex + 0] = vertexOffset + j;
                            triangles[triangleIndex + 1] = cidx1 | (vertexOffset + j & 0xffff); // BitUtil.SetHigh1Low3(1, vertexOffset + j);
                            triangles[triangleIndex + 2] = cidx2 | (vertexOffset + j & 0xffff); // BitUtil.SetHigh1Low3(2, vertexOffset + j);
                            triangles[triangleIndex + 3] = cidx2 | (vertexOffset + j & 0xffff); // BitUtil.SetHigh1Low3(2, vertexOffset + j);
                            triangles[triangleIndex + 4] = cidx3 | (vertexOffset + j & 0xffff); // BitUtil.SetHigh1Low3(3, vertexOffset + j);
                            triangles[triangleIndex + 5] = vertexOffset + j;
                            triangleIndex += 6;
                        }
                        
                        vertexOffset += meshInfo.vertexCount;
                        
                    }
                    
                    indirectArgs.Add(new IndirectArg() {
                        instanceCount = 1,
                        baseVertexLocation = 0,
                        startIndexLocation = indexRangeStart,
                        indexCountPerInstance = triangleIndex - indexRangeStart,
                        startInstanceLocation = 0
                    });

                } 

            }

        }

    }

}