using System;
using Packages.UIForia.Util.Unsafe;
using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Graphics {

    [Flags]
    public enum MeshInfoType {

        Element = 1 << 0,
        Text = 1 << 1,
        Unity = 1 << 2,
        Geometry = 1 << 3

    }

    // needs to be cache line size since I build these in parallel (avoid false sharing)
    [AssertSize(64)]
    public unsafe struct MeshInfo {

        public int meshId;
        public MeshInfoType type;

        public int vertexStart;
        public int vertexCount;

        public int triangleStart;
        public int triangleCount;

        public void* geometryData;
        public int* triangleData;
        private fixed byte padding[19];

    }

    public unsafe struct ElementDesc {

        public float4 positions;
        public int materialIndex;
        public int textureIndex;

    }

    public struct ElementVertex {

        public float2 position;
        public float2 texCoords;
        public int4 indices;

    }

    [BurstCompile]
    internal unsafe struct BakeUIForiaElements : IJob {

        public DataList<MeshInfo> meshInfoList;
        public DataList<DrawInfo2>.Shared drawList;
        public DataList<UIForiaVertex>.Shared vertexList;
        public DataList<ElementMaterialInfo>.Shared materialList;
        
        public void Execute() {
            
            int drawListSize = drawList.size;
            DrawInfo2* drawInfoArray = drawList.GetArrayPointer();
            MeshInfo* meshInfoArray = meshInfoList.GetArrayPointer();

            // its only 1 entry per element, we wont have thousands, and if we do most of them are probably elements anyway
            vertexList.EnsureAdditionalCapacity(drawListSize);
            UIForiaVertex* vertices = vertexList.GetArrayPointer();

            int vertexIdx = 0;
            
            for (int i = 0; i < drawListSize; i++) {

                ref DrawInfo2 drawInfo = ref drawInfoArray[i];

                if (drawInfo.drawType != DrawType2.UIForiaElement) {
                    continue;
                }

                // todo -- handle 9 slicing and batched draws
                ref MeshInfo meshInfo = ref meshInfoArray[i];
                RenderContext3.ElementBatch* elementBatch = (RenderContext3.ElementBatch*) drawInfo.shapeData;

                int count = elementBatch->count;
                
                if (count == 1) {
                    
                    meshInfo.type = MeshInfoType.Element;
                    meshInfo.vertexStart = vertexIdx;
                    meshInfo.vertexCount = 1;

                    int materialIdx = materialList.size;
                    // mat.backgroundColor = 
                    // elementBatch->elements[0].material
                    // int hash = MurmurHash3.Hash((byte*)0, sizeof(ElementMaterialInfo));
                    // FindIdx(hash, ptr, materialList);
                    // materialList.Add();
                    // want to collect data in the material desc but then extract a lot of it to per-element data to cut down on upload 
                    // otherwise too much of the abstraction leaks out, its nice having one material per element / draw
                    // can diff this on texture but no point doing it more than that
                    
                    ref ElementDrawInfo element = ref elementBatch->elements[0];
                    float x = element.x;
                    float y = element.y;
                    float width = element.drawDesc.width;
                    float height = element.drawDesc.height;
                    float halfWidth = width * 0.5f;
                    float halfHeight = height * 0.5f;

                    ElementMaterialInfo mat = default;
                    
                    mat.backgroundColor = element.drawDesc.backgroundColor;
                    mat.backgroundTint = element.drawDesc.backgroundTint;
                    
                    mat.radius0 = element.drawDesc.radiusTL;
                    mat.radius1 = element.drawDesc.radiusTR;
                    mat.radius2 = element.drawDesc.radiusBR;
                    mat.radius3 = element.drawDesc.radiusBL;
                    
                    mat.bevelTL = element.drawDesc.bevelTL;
                    mat.bevelTR = element.drawDesc.bevelTR;
                    mat.bevelBR = element.drawDesc.bevelBR;
                    mat.bevelBL = element.drawDesc.bevelBL;

                    mat.bodyColorMode = element.drawDesc.bgColorMode;
                    mat.outlineColorMode = element.drawDesc.outlineColorMode;
                    
                    mat.outlineWidth = element.drawDesc.outlineWidth;
                    mat.outlineColor = element.drawDesc.outlineColor;
                    
                    materialList.Add(mat);
                    
                    ref UIForiaVertex vertex = ref vertices[vertexIdx++];
                    vertex.position.x = x + halfWidth;
                    vertex.position.y = -(y + halfHeight);
                    vertex.texCoord0.x = width;
                    vertex.texCoord0.y = height;
                    vertex.indices.x = 0; // set later
                    vertex.indices.y = (uint)materialIdx;
                    vertex.indices.z = 0;
                    vertex.indices.w = 0;
                    
                }
                
                // for(int j = 0; j < count; j++) {
                    // todo -- ensure size n stuff
                // }

                // todo -- account for uv rect, background fit, etc
                // might be able to do this in shader
                // for (int v = meshInfo.vertexStart; v < vEnd; v++) {
                //     ref UIForiaVertex vertex = ref vertices[v];
                //     float uvX = ((vertex.position.x) / width);
                //     float uvY = 1 - ((vertex.position.y) / -height);
                //     int hSign = vertex.position.x == 0 ? -1 : 1;
                //     int vSign = vertex.position.y == 0 ? 1 : -1;
                //     vertex.texCoord0.x = uvX + ((0.5f / width) * hSign);
                //     vertex.texCoord0.y = uvY + ((0.5f / height) * vSign);
                //
                //     //vertex.texCoord1.x = uvX;
                //     //vertex.texCoord1.y = uvY;
                // }

            }

            vertexList.size = vertexIdx;
        }

    }

}