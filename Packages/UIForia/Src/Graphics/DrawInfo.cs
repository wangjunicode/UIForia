using System;
using System.Runtime.InteropServices;
using UIForia.Systems;
using Unity.Mathematics;

namespace UIForia.Graphics {

    [Flags]
    internal enum DrawInfoFlags {

        InitialBatchSet = 1 << 0,
        HasMaterialOverrides = 1 << 1,
        HasNonTextureOverrides = 1 << 2,
        FinalBatchSet = 1 << 3,
        Hidden = 1 << 4

    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct DrawInfo {

        // todo -- split this into multiple structs  
        // batching data
        // flags / is-batch-set 
        // material
        // properties
        // maybe vertex layout
        // overflow bounds
        // clipper id
        // maybe aabb

        // sorting data
        // local index, render call id

        // geometry data
        // geometry
        // matrix
        // shape data
        // 

        public DrawType type;
        public DrawInfoFlags flags;

        public int renderCallId;
        public int localDrawIdx;

        public int materialOverrideCount;

        public GeometryInfo* geometryInfo;

        public VertexLayout vertexLayout;
        public MaterialId materialId;

        public AxisAlignedBounds2D aabb;
        public byte* shapeData;
        public float4x4* matrix;
        public MaterialPropertyOverride* materialOverrideValues;
        public ElementId elementId;
        public OverflowBounds* overflowBounds;
        public int overflowBoundRenderIndex;

        public void* GetChannel(VertexChannel channel) {

            if ((type & (DrawType.SDFText | DrawType.Shape)) != 0) {

                switch (channel) {

                    case VertexChannel.Position:
                        return (byte*) geometryInfo->positions;

                    case VertexChannel.Color:
                        return (byte*) geometryInfo->colors;

                    case VertexChannel.TextureCoord0:
                        return (byte*) geometryInfo->texCoord0;

                    case VertexChannel.TextureCoord1:
                        if (geometryInfo->vertexLayout.texCoord1 != VertexChannelFormat.Off) {
                            return geometryInfo->texCoord1;
                        }

                        break;

                    case VertexChannel.Normal:
                        if (geometryInfo->vertexLayout.normal != VertexChannelFormat.Off) {
                            return geometryInfo->normal;
                        }

                        break;

                    case VertexChannel.Tangent: {
                        if (geometryInfo->vertexLayout.tangent != VertexChannelFormat.Off) {
                            return geometryInfo->tangent;
                        }

                        break;
                    }

                    case VertexChannel.TextureCoord2: {
                        if (geometryInfo->vertexLayout.texCoord2 != VertexChannelFormat.Off) {
                            return geometryInfo->texCoord2;
                        }

                        break;
                    }

                    case VertexChannel.TextureCoord3: {
                        if (geometryInfo->vertexLayout.texCoord3 != VertexChannelFormat.Off) {
                            return geometryInfo->texCoord3;
                        }

                        break;
                    }

                    case VertexChannel.TextureCoord4: {
                        if (geometryInfo->vertexLayout.texCoord4 != VertexChannelFormat.Off) {
                            return geometryInfo->texCoord3;
                        }

                        break;
                    }

                    case VertexChannel.TextureCoord5:
                    case VertexChannel.TextureCoord6:
                    case VertexChannel.TextureCoord7:

                    default:
                        throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(channel), channel, null);

        }

        public int* GetTriangles() {

            if ((type & (DrawType.SDFText | DrawType.Shape)) != 0) {
                return geometryInfo->triangles;
            }

            return null;
        }

    }

}