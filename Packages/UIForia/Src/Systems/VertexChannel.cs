using System;

namespace UIForia.Systems {

    [Flags]
    public enum VertexChannel {

        Position = 0,
        Normal = 1 << 0,
        Color = 1 << 1,
        Tangent = 1 << 2,
        TextureCoord0 = 1 << 3,
        TextureCoord1 = 1 << 4,
        TextureCoord2 = 1 << 5,
        TextureCoord3 = 1 << 6,
        TextureCoord4 = 1 << 7,
        TextureCoord5 = 1 << 8,
        TextureCoord6 = 1 << 9,
        TextureCoord7 = 1 << 10,

    }

}