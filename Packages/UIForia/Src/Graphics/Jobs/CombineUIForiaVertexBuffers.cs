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
        
        public DataList<int>.Shared elementTriangleList;
        public DataList<int>.Shared textTriangleList;
        
        public List_Int32 matrixIdList;
        public List_Int32 clipRectIdList;
        public List_Int32 materialIdList;

        public void Execute() {

            int elementVertexOffset = textVertexList.size;
            int elementTriangleOffset = textTriangleList.size;

            textVertexList.EnsureAdditionalCapacity(elementVertexList.size);
            textVertexList.AddRange(elementVertexList.GetArrayPointer(), elementVertexList.size);

            textTriangleList.EnsureAdditionalCapacity(elementTriangleList.size);
            textTriangleList.AddRange(elementTriangleList.GetArrayPointer(), elementTriangleList.size);

            UIForiaVertex* vertices = textVertexList.GetArrayPointer();
            int* triangles = textTriangleList.GetArrayPointer();

            const MeshInfoType targetType = MeshInfoType.Element | MeshInfoType.Text;

            for (int i = 0; i < meshList.size; i++) {

                ref MeshInfo meshInfo = ref meshList[i];

                if ((meshInfo.type & targetType) == 0) {
                    continue;
                }

                ushort materialId = (ushort) materialIdList.array[i];
                ushort matrixId = (ushort) matrixIdList.array[i];
                ushort clipRectId = (ushort) clipRectIdList.array[i];

                int vertexOffset = 0;

                if (meshInfo.type == MeshInfoType.Element) {
                    vertexOffset = elementVertexOffset;
                }

                int start = meshInfo.vertexStart + vertexOffset;
                int end = start + meshInfo.vertexCount;

                int2 indices = new int2(BitUtil.SetHighLowBits(materialId, matrixId), BitUtil.SetHighLowBits(clipRectId, 0));

                for (int v = start; v < end; v++) {
                    vertices[v].indices = indices;
                }
            }
            
            // since we appended the element list to the text list we need to add an
            // offset to all of the element triangles so they point to the right place
            
            for (int i = 0; i < meshList.size; i++) {

                ref MeshInfo meshInfo = ref meshList[i];

                if (meshInfo.type != MeshInfoType.Element) {
                    continue;
                }

                // need to update these so the index building job has the correct offset
                meshInfo.vertexStart += elementVertexOffset;
                meshInfo.triangleStart += elementTriangleOffset;
                
                int start = meshInfo.triangleStart;
                int end = start + meshInfo.triangleCount;
                
                for (int t = start; t < end; t++) {
                    triangles[t] += elementVertexOffset;
                }
            }

        }

    }

}