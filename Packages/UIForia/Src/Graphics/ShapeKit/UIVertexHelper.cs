using System;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics.ShapeKit {

    public struct ShapeRange {

        public RangeInt vertexRange;
        public RangeInt triangleRange;

    }

    public unsafe struct UIVertexHelper : IDisposable {

        private Allocator allocator;

        private int vertexCount;
        private int triangleCount;

        public float3* positions;
        public float4* texCoord;
        public Color32* colors;
        public int* triangles;

        // private NativeArray<float3> positionArray;
        // private NativeArray<float4> uvArray;
        // private NativeArray<Color32> colorArray;
        // private NativeArray<int> trianglesArray;

        private int currentShapeVertexStart;
        private int currentShapeTriangleStart;

        private int vertexCapacity;
        private int triangleCapacity;

        public void BeginShape() {
            currentShapeTriangleStart = triangleCount;
            currentShapeVertexStart = vertexCount;
            shapeMode = true;
        }

        public ShapeRange EndShape() {
            shapeMode = false;
            return new ShapeRange() {
                vertexRange = new RangeInt(currentShapeVertexStart, vertexCount - currentShapeVertexStart),
                triangleRange = new RangeInt(currentShapeTriangleStart, triangleCount - currentShapeTriangleStart)
            };
        }

        public bool shapeMode;

        public int currentVertCount {
            get => shapeMode ? vertexCount - currentShapeVertexStart : vertexCount;
        }

        public int TotalVertexCount {
            get => vertexCount;
        }

        public int TotalTriangleCount {
            get => triangleCount;
        }

        public void AddVertexCount(int additional) {
            vertexCount += additional;
        }

        public void Dispose() {
            TypedUnsafe.Dispose(positions, allocator);
            TypedUnsafe.Dispose(triangles, allocator);
            this = default;
        }

        public void AddVert(in Vector3 position, Color32 color, in Vector2 uv) {
            if (vertexCount + 1 >= vertexCapacity) {
                EnsureAdditionalVertexCapacity(1);
            }

            positions[vertexCount] = position;
            colors[vertexCount] = color;
            texCoord[vertexCount] = new float4(uv.x, uv.y, 0, 0);
            vertexCount++;
        }

        public void AddVert(in Vector3 position, Color32 color, in float4 uv) {
            if (vertexCount + 1 >= vertexCapacity) {
                EnsureAdditionalVertexCapacity(1);
            }

            positions[vertexCount] = position;
            colors[vertexCount] = color;
            texCoord[vertexCount] = uv;
            vertexCount++;
        }

        public void AddVert(in float3 position, Color32 color, in float4 uv) {
            if (vertexCount + 1 >= vertexCapacity) {
                EnsureAdditionalVertexCapacity(1);
            }

            positions[vertexCount] = position;
            colors[vertexCount] = color;
            texCoord[vertexCount] = uv;
            vertexCount++;
        }

        public void AddVert(in float3 position, Color32 color, in float2 uv) {
            if (vertexCount + 1 >= vertexCapacity) {
                EnsureAdditionalVertexCapacity(1);
            }

            positions[vertexCount] = position;
            colors[vertexCount] = color;
            texCoord[vertexCount] = new float4(uv.x, uv.y, 0, 0);
            vertexCount++;
        }

        public void AddTriangleUnchecked(int a, int b, int c) {
            triangles[triangleCount++] = a;
            triangles[triangleCount++] = b;
            triangles[triangleCount++] = c;
        }

        public void AddTriangle(int a, int b, int c) {
            if (triangleCount + 3 > triangleCapacity) {
                TypedUnsafe.Resize(ref triangles, triangleCapacity, math.max(128, triangleCapacity * 2), allocator);
            }

            triangles[triangleCount++] = a;
            triangles[triangleCount++] = b;
            triangles[triangleCount++] = c;
        }

        public void Clear() {
            vertexCount = 0;
            triangleCount = 0;
            shapeMode = false;
            currentShapeTriangleStart = 0;
            currentShapeVertexStart = 0;
        }

        public static UIVertexHelper Create(Allocator allocator) {

            TypedUnsafe.MallocSplitBuffer(out float3* positions, out float4* texCoords, out Color32* colors, 128, allocator);

            int* triangles = TypedUnsafe.Malloc<int>(256, allocator);

            return new UIVertexHelper() {
                allocator = allocator,

                colors = colors,
                positions = positions,
                triangles = triangles,
                texCoord = texCoords,
                triangleCount = 0,
                vertexCount = 0,
                triangleCapacity = 256,
                vertexCapacity = 128
            };
        }

        public void FillMesh(Mesh mesh) {
            mesh.Clear();

            NativeArray<float3> positionArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float3>(positions, vertexCount, allocator);
            NativeArray<float4> uvArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float4>(texCoord, vertexCount, allocator);
            NativeArray<Color32> colorArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Color32>(colors, vertexCount, allocator);
            NativeArray<int> trianglesArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(triangles, triangleCount, allocator);

            mesh.SetVertices(positionArray, 0, vertexCount);
            mesh.SetUVs(0, uvArray, 0, vertexCount);
            mesh.SetColors(colorArray, 0, vertexCount);
            mesh.SetIndices(trianglesArray, 0, triangleCount, MeshTopology.Triangles, 0, false);
        }

        public void EnsureAdditionalVertexCapacity(int add) {
            if (vertexCount + add <= vertexCapacity) {
                return;
            }

            int addSize = math.max((vertexCount + add) * 2, vertexCapacity * 2);

            TypedUnsafe.ResizeSplitBuffer(ref positions, ref texCoord, ref colors, vertexCount, addSize, allocator);

            vertexCapacity = addSize;

        }

        public void EnsureAdditionalTriangleCapacity(int size) {
            if (triangleCount + size <= triangleCapacity) {
                return;
            }

            int addSize = math.max((triangleCount + size) * 2, triangleCapacity * 2);

            TypedUnsafe.Resize(ref triangles, triangleCapacity, addSize, allocator);
            triangleCapacity = addSize;
        }

    }

}