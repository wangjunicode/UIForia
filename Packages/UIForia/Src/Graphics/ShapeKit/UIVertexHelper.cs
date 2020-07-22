using System;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics.ShapeKit {
    

    public unsafe struct UIVertexHelper : IDisposable {

        private Allocator allocator;

        private int vertexCount;
        private int triangleCount;

        public float3* positions;
        public float4* texCoord;
        public Color32* colors;
        public int* triangles;

        private int vertexCapacity;
        private int triangleCapacity;

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
        
        public int currentVertCount {
            get => vertexCount;
        }

        public int TotalVertexCount {
            get => vertexCount;
        }

        public int TotalTriangleCount {
            get => triangleCount;
        }

        public void AddVertexCount(int count) {
            vertexCount += count;
        }

        public void AddTriangleCount(int count) {
            triangleCount += count;
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
            if (triangleCount + 3 >= triangleCapacity) {
                EnsureAdditionalTriangleCapacity(3);
            }

            triangles[triangleCount++] = a;
            triangles[triangleCount++] = b;
            triangles[triangleCount++] = c;
        }

        public void Clear() {
            vertexCount = 0;
            triangleCount = 0;
        }

        public void FillMesh(Mesh mesh) {
            mesh.Clear();

            NativeArray<float3> positionArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float3>(positions, vertexCount, allocator);
            NativeArray<float4> uvArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float4>(texCoord, vertexCount, allocator);
            NativeArray<Color32> colorArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Color32>(colors, vertexCount, allocator);
            NativeArray<int> trianglesArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(triangles, triangleCount, allocator);
            NativeArray<int> dummy = new NativeArray<int>(1, Allocator.Temp);
#if UNITY_EDITOR
            AtomicSafetyHandle safetyHandle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(dummy);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref positionArray, safetyHandle);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref uvArray, safetyHandle);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref colorArray, safetyHandle);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref trianglesArray, safetyHandle);
#endif
            mesh.SetVertices(positionArray, 0, vertexCount);
            mesh.SetUVs(0, uvArray, 0, vertexCount);
            mesh.SetColors(colorArray, 0, vertexCount);
            mesh.SetIndices(trianglesArray, 0, triangleCount, MeshTopology.Triangles, 0, false);
            dummy.Dispose();

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
            if (triangleCount + size < triangleCapacity) {
                return;
            }

            int addSize = math.max((triangleCount + size) * 2, triangleCapacity * 2);
            int* newptr = (int*) UnsafeUtility.Malloc(addSize * sizeof(int), 4, allocator);
            UnsafeUtility.MemCpy(newptr, triangles, sizeof(int) * triangleCount);
            UnsafeUtility.Free(triangles, allocator);

            triangles = newptr;
            triangleCapacity = addSize;
        }

    }

}