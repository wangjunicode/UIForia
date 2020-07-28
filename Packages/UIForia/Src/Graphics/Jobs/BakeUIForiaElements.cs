using System;
using System.Diagnostics;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    [Flags]
    public enum MeshInfoType {

        Element = 1 << 0,
        Text = 1 << 1,
        Unity = 1 << 2,
        Geometry = 1 << 3

    }

    // needs to be cache line size since I build these in parallel
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

    [BurstCompile]
    internal unsafe struct BakeUIForiaShapes : IJob {

        public DataList<DrawInfo2>.Shared drawList;
        public DataList<int>.Shared triangleList;
        public DataList<UIForiaVertex>.Shared vertexList;
        public DataList<MeshInfo> meshInfoList;

        public void Execute() { }

    }

    [BurstCompile]
    internal unsafe struct BakeUIForiaElements : IJob {

        // todo -- make sure geometry type is at least 1 cache line in size 
        public DataList<DrawInfo2>.Shared drawList;
        public DataList<MeshInfo> meshInfoList;

        public DataList<UIForiaVertex>.Shared vertexList;

        [NativeDisableUnsafePtrRestriction] private float2* scratchFloats;

        public void Execute() {

            scratchFloats = TypedUnsafe.Malloc<float2>(4, Allocator.Temp);

            List_float2 tempVertexBuffer = new List_float2(32, Allocator.Temp);

            int drawListSize = drawList.size;
            DrawInfo2* drawInfoArray = drawList.GetArrayPointer();
            MeshInfo* meshInfoArray = meshInfoList.GetArrayPointer();
            UIForiaVertex* vertices = null;

            // todo -- can pre-allocate vertex space since we now only support quads, this is easy
            for (int i = 0; i < drawListSize; i++) {

                ref DrawInfo2 drawInfo = ref drawInfoArray[i];

                if (drawInfo.drawType != DrawType2.UIForiaElement) {
                    continue;
                }

                ref MeshInfo meshInfo = ref meshInfoArray[i];

                meshInfo.type = MeshInfoType.Element;

                meshInfo.vertexStart = vertexList.size;

                SDFMeshDesc* desc = (SDFMeshDesc*) drawInfo.shapeData;

                float x = desc->x;
                float y = desc->y;
                float width = desc->width;
                float height = desc->height;

                // should never happen but avoids a div by 0 just in case
                if (width <= 0 || height <= 0) {
                    meshInfo.vertexCount = 0;
                    meshInfo.triangleCount = 0;
                    continue;
                }

                // todo -- for proper pivot I probably want to create mesh with pivot position in mind
                vertexList.EnsureAdditionalCapacity(4);
                vertices = vertexList.GetArrayPointer();

                int vertexStart = vertexList.size;

                vertices[vertexStart + 0] = new UIForiaVertex(x, y); // tl
                vertices[vertexStart + 1] = new UIForiaVertex(x + width, y); // tr
                vertices[vertexStart + 2] = new UIForiaVertex(x + width, -(y + height)); // br
                vertices[vertexStart + 3] = new UIForiaVertex(x, -(y + height)); // bl

                vertexList.size += 4;

                meshInfo.vertexCount = vertexList.size - meshInfo.vertexStart;
                int vEnd = meshInfo.vertexStart + meshInfo.vertexCount;

                vertices = vertexList.GetArrayPointer();

                // todo -- account for uv rect, background fit, etc
                // might be able to do this in shader
                for (int v = meshInfo.vertexStart; v < vEnd; v++) {
                    ref UIForiaVertex vertex = ref vertices[v];
                    float uvX = ((vertex.position.x) / width);
                    float uvY = 1 - ((vertex.position.y) / -height);
                    int hSign = vertex.position.x == 0 ? -1 : 1;
                    int vSign = vertex.position.y == 0 ? 1 : -1;
                    vertex.texCoord0.x = uvX + ((0.5f / width) * hSign);
                    vertex.texCoord0.y = uvY + ((0.5f / height) * vSign);

                    //vertex.texCoord1.x = uvX;
                    //vertex.texCoord1.y = uvY;
                }

            }

            TypedUnsafe.Dispose(scratchFloats, Allocator.Temp);
            tempVertexBuffer.Dispose();
        }

    }

}