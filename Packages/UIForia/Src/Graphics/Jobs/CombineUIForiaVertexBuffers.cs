using UIForia.ListTypes;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct CombineUIForiaVertexBuffers : IJob {

        public DataList<MeshInfo> meshList;

        public DataList<UIForiaVertex>.Shared elementVertexList;
        public DataList<UIForiaVertex>.Shared textVertexList;

        public List_Int32 matrixIdList;
        public List_Int32 clipRectIdList;

        public void Execute() {

            textVertexList.EnsureAdditionalCapacity(elementVertexList.size);
            textVertexList.AddRange(elementVertexList.GetArrayPointer(), elementVertexList.size);

            UIForiaVertex* vertices = textVertexList.GetArrayPointer();

            for (int i = 0; i < meshList.size; i++) {

                ref MeshInfo meshInfo = ref meshList[i];

                // todo -- reimplement elements here
                if (meshInfo.type == MeshInfoType.Text) {
                    int start = meshInfo.vertexStart;
                    int end = start + meshInfo.vertexCount;

                    int matrixId = matrixIdList.array[i];
                    int clipRectId = clipRectIdList.array[i];
                    uint clipAndMatrix = (uint)BitUtil.SetHighLowBits(clipRectId, matrixId);
                    
                    for (int v = start; v < end; v++) {
                        vertices[v].indices.x = clipAndMatrix;
                    }
                }
                
                // if ((meshInfo.type & targetType) == 0) {
                //     continue;
                // }
                //
                // // not a lot of need for materialIdList I think
                // // text handles it differently anyway
                // // and elements are 1-1 and maaaybe dont even use deduplication
                //
                // ushort materialId = (ushort) materialIdList.array[i];
                // ushort matrixId = (ushort) matrixIdList.array[i];
                // ushort clipRectId = (ushort) clipRectIdList.array[i];
                //
                // int vertexOffset = 0;
                //
                // if (meshInfo.type == MeshInfoType.Element) {
                //     vertexOffset = elementVertexOffset;
                // }
                //
                // int start = meshInfo.vertexStart + vertexOffset;
                // int end = start + meshInfo.vertexCount;
                //
                // // i need to squeeze uv transform in somewhere. 
                // // maybe use the texture coords and look it up later elsewhere?
                // // or use an int3 for indices.. can afford the space for sure
                // int2 indices = new int2(BitUtil.SetHighLowBits(materialId,  0), BitUtil.SetHighLowBits(clipRectId, matrixId));
                //
                // for (int v = start; v < end; v++) {
                //     vertices[v].indices = indices;
                // }
            }

        }

    }

}