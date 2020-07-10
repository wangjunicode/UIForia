using UIForia.Rendering.Vertigo;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace UIForia.Rendering {

    internal struct Batch {

        public int drawCallSize;
        public BatchType batchType;
        public PooledMesh pooledMesh;
        public MaterialPropertyBlock propertyBlock;
        public Mesh unpooledMesh;
        public Material material;
        public UIForiaData uiforiaData;
        public StructList<Matrix4x4> transformData;
        public int renderStateId;

    }

}

namespace UIForia.Graphics {

    public unsafe struct Batch {

        public BatchType batchType;
        public RangeInt memberIdRange;
        public int meshId;
        public VertexLayout vertexLayout;
        public int vertexChannelCount;
        public int vertexCount;
        public int triangleCount;
        public VertexChannelDesc* geometry;
        public int* triangles;
        public MaterialId materialId;
        public MaterialPropertyOverride* propertyOverrides;
        public int propertyOverrideCount;

        public int overflowBoundCount;
        public OverflowBounds* overflowBounds;
        public byte* geometryInterleaved;
        public StencilState stencilState;
        
        public bool HasUIForiaMaterial() {
            return true;
        }

        // public float3* GetPositions() {
        //
        //     for (int i = 0; i < vertexChannelCount; i++) {
        //         VertexChannelDesc channelDesc = *(geometry + i);
        //         if (channelDesc.channel == VertexChannel.Position) {
        //             return (float3*) channelDesc.ptr;
        //         }
        //     }
        //
        //     return null;
        //
        // }
        //
        // public float4* GetTexCoord0() {
        //     for (int i = 0; i < vertexChannelCount; i++) {
        //         VertexChannelDesc channelDesc = *(geometry + i);
        //         if (channelDesc.channel == VertexChannel.TextureCoord0) {
        //             return (float4*) channelDesc.ptr;
        //         }
        //     }
        //
        //     return null;
        // }
        //
        // public float4* GetTexCoord1() {
        //     for (int i = 0; i < vertexChannelCount; i++) {
        //         VertexChannelDesc channelDesc = *(geometry + i);
        //         if (channelDesc.channel == VertexChannel.TextureCoord1) {
        //             return (float4*) channelDesc.ptr;
        //         }
        //     }
        //
        //     return null;
        // }
        //
        // public Color32* GetColors() {
        //     for (int i = 0; i < vertexChannelCount; i++) {
        //         VertexChannelDesc channelDesc = *(geometry + i);
        //         if (channelDesc.channel == VertexChannel.Color) {
        //             return (Color32*) channelDesc.ptr;
        //         }
        //     }
        //
        //     return null;
        // }

    }

}