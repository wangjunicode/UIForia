using UIForia.Systems;

namespace UIForia.Graphics {

    public unsafe struct VertexChannelDesc {

        public VertexChannel channel;
        public VertexChannelFormat format;
        public void* ptr;

        public int GetUVChannelIndex() {
            switch (channel) {
                default:
                case VertexChannel.Normal:
                case VertexChannel.Color:
                case VertexChannel.Tangent:
                    return -1;

                case VertexChannel.TextureCoord0:
                    return 0;

                case VertexChannel.TextureCoord1:
                    return 1;

                case VertexChannel.TextureCoord2:
                    return 2;

                case VertexChannel.TextureCoord3:
                    return 3;

                case VertexChannel.TextureCoord4:
                    return 4;

                case VertexChannel.TextureCoord5:
                    return 5;

                case VertexChannel.TextureCoord6:
                    return 6;

                case VertexChannel.TextureCoord7:
                    return 7;
            }
        }

    }

}