using UIForia.Systems;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Graphics {

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
            // texCoord1 = VertexChannelFormat.Float4
        };

        public static VertexLayout UIForiaSDFText = new VertexLayout() {
            color = VertexChannelFormat.Float1,
            texCoord0 = VertexChannelFormat.Float4,
        };

    }

}