using System;
using UIForia.Graphics.ShapeKit;
using UIForia.Systems;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Graphics {

    public enum VertexFormat {

        DefaultUI,
        UIForiaText,
        UIForiaShape,
        Custom

    }


    public unsafe struct VertexLayout {

        public VertexChannelFormat texCoord0;
        public VertexChannelFormat texCoord1;
        public VertexChannelFormat texCoord2;
        public VertexChannelFormat texCoord3;
        
        public VertexChannelFormat texCoord4;
        public VertexChannelFormat texCoord5;
        public VertexChannelFormat texCoord6;
        public VertexChannelFormat texCoord7;
        
        public VertexChannelFormat tangent;
        public VertexChannelFormat normal;
        public VertexChannelFormat color;
        public VertexChannelFormat padding;

        public static bool Equal(VertexLayout a, VertexLayout b) {
            return UnsafeUtility.MemCmp(&a, &b, sizeof(VertexLayout)) == 0;
        }

        public static readonly VertexLayout UIForiaDefault = new VertexLayout() {
            color = VertexChannelFormat.Float1,
            texCoord0 = VertexChannelFormat.Float4,
        };

    }

    internal unsafe struct DrawInfo {

        public DrawType type; // flags?
        public int materialKeyIndex;
        public int renderCallIdx; // can use a larger number space and combine localDrawIdx with this
        public int localDrawIdx;
        public int batchId;

        public UIVertexHelper* shapeGeometrySource;
        public ShapeRange shapeRange;
        public byte* shapeData;
        public byte* modifiedVertexData;
        public float4x4 * matrix;
        public VertexLayout vertexLayout;

        public byte* GetChannel(VertexChannel channel) {

            if ((type & DrawType.Shape) != 0) {

                switch (channel) {

                    case VertexChannel.Position:
                        return (byte*) (shapeGeometrySource->positions + shapeRange.vertexRange.start);

                    case VertexChannel.Color:
                        return (byte*) (shapeGeometrySource->colors + shapeRange.vertexRange.start);

                    case VertexChannel.TextureCoord0:
                        return (byte*) (shapeGeometrySource->texCoord + shapeRange.vertexRange.start);

                    case VertexChannel.Normal:
                    case VertexChannel.Tangent:
                    case VertexChannel.TextureCoord1:
                    case VertexChannel.TextureCoord2:
                    case VertexChannel.TextureCoord3:
                    case VertexChannel.TextureCoord4:
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
            
            if ((type & DrawType.Shape) != 0) {
                return shapeGeometrySource->triangles + shapeRange.triangleRange.start;
            }

            return null;
        }

    }

}