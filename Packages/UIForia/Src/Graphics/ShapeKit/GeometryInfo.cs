using System.Diagnostics;
using UIForia.Systems;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Graphics {

    [DebuggerTypeProxy(typeof(GeometryInfoDebugView))]
    public unsafe struct GeometryInfo {

        public VertexLayout vertexLayout;
        
        public AxisAlignedBounds2D bounds; 
        
        public float3* positions;
        public float4* texCoord0;
        public Color32* colors;
        public int* triangles;

        public void* interleaved;
        
        public void* normal;
        public void* tangent;

        public void* texCoord1;
        public void* texCoord2;
        public void* texCoord3;
        public void* texCoord4;

        public int vertexCount;
        public int triangleCount;

    }

    public struct GeometryInfoDebugView {

        public float3[] positions;
        public float4[] texCoords;
        public Color32[] colors;
        public int[] triangles;
        public int vertexCount;
        public int triangleCount;

        public unsafe GeometryInfoDebugView(in GeometryInfo geo) {

            positions = new float3[geo.vertexCount];
            texCoords = new float4[geo.vertexCount];
            colors = new Color32[geo.vertexCount];
            triangles = new int[geo.triangleCount];

            vertexCount = geo.vertexCount;
            triangleCount = geo.triangleCount;

            for (int i = 0; i < geo.triangleCount; i++) {
                triangles[i] = geo.triangles[i];
            }

            for (int i = 0; i < geo.vertexCount; i++) {
                positions[i] = geo.positions[i];
                texCoords[i] = geo.texCoord0[i];
                colors[i] = geo.colors[i];
            }

        }

    }

}