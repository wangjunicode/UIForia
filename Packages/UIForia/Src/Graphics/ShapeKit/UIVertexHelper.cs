using System;
using System.Collections.Generic;
using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics.ShapeKit {

    public unsafe struct UIVertexHelper : IDisposable {

        private Allocator allocator;

        public int vertexCount;
        public int triangleCount;

        public float3* positions;
        public float4* uv0;
        public Color32* colors;
        public int* triangles;

        private NativeArray<float3> positionArray;
        private NativeArray<float4> uvArray;
        private NativeArray<Color32> colorArray;
        private NativeArray<int> trianglesArray;

        public int currentVertCount {
            get => vertexCount;
        }

        public void Dispose() {
            positionArray.Dispose();
            uvArray.Dispose();
            colorArray.Dispose();
            trianglesArray.Dispose();
            this = default;
        }

        public void AddVert(in Vector3 position, Color32 color, in Vector2 uv) {
            if (vertexCount + 1 >= positionArray.Length) {
                EnsureAdditionalVertexCapacity(1);
            }

            positions[vertexCount] = position;
            colors[vertexCount] = color;
            uv0[vertexCount] = new float4(uv.x, uv.y, 0, 0);
            vertexCount++;
        }

        public void AddVert(in Vector3 position, Color32 color, in float4 uv) {
            if (vertexCount + 1 >= positionArray.Length) {
                EnsureAdditionalVertexCapacity(1);
            }

            positions[vertexCount] = position;
            colors[vertexCount] = color;
            uv0[vertexCount] = uv;
            vertexCount++;
        }

        public void AddVert(in float3 position, Color32 color, in float4 uv) {
            if (vertexCount + 1 >= positionArray.Length) {
                EnsureAdditionalVertexCapacity(1);
            }

            positions[vertexCount] = position;
            colors[vertexCount] = color;
            uv0[vertexCount] = uv;
            vertexCount++;
        }

        public void AddVert(in float3 position, Color32 color, in float2 uv) {
            if (vertexCount + 1 >= positionArray.Length) {
                EnsureAdditionalVertexCapacity(1);
            }

            positions[vertexCount] = position;
            colors[vertexCount] = color;
            uv0[vertexCount] = new float4(uv.x, uv.y, 0, 0);
            vertexCount++;

        }

        public void AddTriangleUnchecked(int a, int b, int c) {
            triangles[triangleCount++] = a;
            triangles[triangleCount++] = b;
            triangles[triangleCount++] = c;
        }

        public void AddTriangle(int a, int b, int c) {
            if (triangleCount + 3 > trianglesArray.Length) {
                NativeArray<int> newArray = new NativeArray<int>(trianglesArray.Length * 2, allocator, NativeArrayOptions.UninitializedMemory);
                TypedUnsafe.MemCpy(
                    (int*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(newArray),
                    (int*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(trianglesArray),
                    triangleCount
                );
                trianglesArray.Dispose();
                trianglesArray = newArray;
                triangles = (int*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(trianglesArray);
            }

            triangles[triangleCount++] = a;
            triangles[triangleCount++] = b;
            triangles[triangleCount++] = c;
        }

        public void Clear() {
            vertexCount = 0;
            triangleCount = 0;
        }

        public static UIVertexHelper Create(Allocator allocator) {
            NativeArray<float3> positions = new NativeArray<float3>(512, allocator, NativeArrayOptions.UninitializedMemory);
            NativeArray<float4> uv = new NativeArray<float4>(512, allocator, NativeArrayOptions.UninitializedMemory);
            NativeArray<Color32> colors = new NativeArray<Color32>(512, allocator, NativeArrayOptions.UninitializedMemory);
            NativeArray<int> triangles = new NativeArray<int>(512 * 3, allocator, NativeArrayOptions.UninitializedMemory);

            return new UIVertexHelper() {
                allocator = allocator,
                positionArray = positions,
                uvArray = uv,
                colorArray = colors,
                trianglesArray = triangles,
                colors = (Color32*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(colors),
                positions = (float3*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(positions),
                triangles = (int*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(triangles),
                uv0 = (float4*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(uv),
                triangleCount = 0,
                vertexCount = 0
            };
        }

        public void FillMesh(Mesh mesh) {
            mesh.Clear();
            mesh.SetVertices(positionArray, 0, vertexCount);
            mesh.SetUVs(0, uvArray, 0, vertexCount);
            mesh.SetColors(colorArray, 0, vertexCount);
            mesh.SetIndices(trianglesArray, 0, triangleCount, MeshTopology.Triangles, 0, false);
        }

        public void EnsureAdditionalVertexCapacity(int add) {
            if (vertexCount + add <= positionArray.Length) {
                return;
            }

            int addSize = math.max((vertexCount + add) * 2, positionArray.Length * 2);

            NativeArray<float3> newPositionsArray = new NativeArray<float3>(addSize, allocator, NativeArrayOptions.UninitializedMemory);
            TypedUnsafe.MemCpy(
                (float3*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(newPositionsArray),
                (float3*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(positionArray),
                vertexCount
            );

            positionArray.Dispose();
            positionArray = newPositionsArray;

            NativeArray<float4> newUVArray = new NativeArray<float4>(addSize, allocator, NativeArrayOptions.UninitializedMemory);
            TypedUnsafe.MemCpy(
                (float4*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(newUVArray),
                (float4*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(uvArray),
                vertexCount
            );

            uvArray.Dispose();
            uvArray = newUVArray;

            NativeArray<Color32> newColorArray = new NativeArray<Color32>(addSize, allocator, NativeArrayOptions.UninitializedMemory);
            TypedUnsafe.MemCpy(
                (Color32*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(newColorArray),
                (Color32*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(colorArray),
                vertexCount
            );

            colorArray.Dispose();
            colorArray = newColorArray;

            positions = (float3*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(positionArray);
            uv0 = (float4*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(uvArray);
            colors = (Color32*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(colorArray);

        }

        public void EnsureAdditionalTriangleCapacity(int size) {
            if (triangleCount + size <= trianglesArray.Length) {
                return;
            }

            int addSize = math.max((triangleCount + size) * 2, trianglesArray.Length * 2);

            NativeArray<int> newArray = new NativeArray<int>(addSize, allocator, NativeArrayOptions.UninitializedMemory);
            TypedUnsafe.MemCpy(
                (int*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(newArray),
                (int*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(trianglesArray),
                triangleCount
            );
            trianglesArray.Dispose();
            trianglesArray = newArray;
            triangles = (int*) NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(trianglesArray);
        }

    }

}