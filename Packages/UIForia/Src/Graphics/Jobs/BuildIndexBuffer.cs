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
            MeshInfo* meshArray = meshInfoList.GetArrayPointer();
            Batch* batchArray = batchList.GetArrayPointer();
            
            int triangleCount = 0;
            for (int batchIdx = 0; batchIdx < batchList.size; batchIdx++) {
                ref Batch batch = ref batchArray[batchIdx];

                if (batch.type == BatchType.Text || batch.type == BatchType.Element || batch.type == BatchType.Shadow) {
                    
                    int start = batch.memberIdRange.start;
                    int end = batch.memberIdRange.end;
                    
                    for (int i = start; i < end; i++) {
                        ref MeshInfo meshInfo = ref meshArray[batchMemberArray[i]];
                        triangleCount += meshInfo.vertexCount * 6;
                    }
                }

            }

            indexBuffer.EnsureCapacity(triangleCount);
            indexBuffer.size = triangleCount;

            int* triangles = indexBuffer.GetArrayPointer();
            int triangleIndex = 0;

            for (int batchIdx = 0; batchIdx < batchList.size; batchIdx++) {

                ref Batch batch = ref batchArray[batchIdx];

                // I guess this includes text decorations, I'm not sure how to handle those since they arent really quads.
                // I can maybe repurpose some data in order to get the uvs to stretch correctly 

                const int cornerIdx1 = 1 << 24;
                const int cornerIdx2 = 2 << 24;
                const int cornerIdx3 = 3 << 24;
                
                // todo -- reverse the batch building to save uploading the index buffer every frame
                // If I can put the vertices in the correct order instead of the indices, I can use a 
                // totally static index buffer and never need to upload it
                if (batch.type == BatchType.Element || batch.type == BatchType.Text || batch.type == BatchType.Shadow) {
                    
                    int start = batch.memberIdRange.start;
                    int end = batch.memberIdRange.end;
                    int indexRangeStart = triangleIndex;

                    for (int i = start; i < end; i++) {
                    
                        ref MeshInfo meshInfo = ref meshArray[batchMemberArray[i]];

                        int vertexOffset = meshInfo.vertexStart;
                        
                        for (int j = 0; j < meshInfo.vertexCount; j++) {
                            // shouldn't need to mask vertex offset with 0xffff because if it overflows
                            // we're screwed anyway (pretty unlikely to need more than 16,777,216 indices)
                            // this will handle 2,796,202 quads
                            int vertIdx = vertexOffset + j;
                            triangles[triangleIndex + 0] = vertIdx;
                            triangles[triangleIndex + 1] = cornerIdx1 | vertIdx;
                            triangles[triangleIndex + 2] = cornerIdx2 | vertIdx;
                            triangles[triangleIndex + 3] = cornerIdx2 | vertIdx;
                            triangles[triangleIndex + 4] = cornerIdx3 | vertIdx;
                            triangles[triangleIndex + 5] = vertIdx;
                            triangleIndex += 6;
                        }
                        
                    }

                    batch.indirectArgOffset = indirectArgs.size;
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