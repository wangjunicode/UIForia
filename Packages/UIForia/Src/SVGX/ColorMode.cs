using System;

namespace SVGX {

    [Flags]
    public enum ColorMode {

        Color = 0,
        Texture = 1 << 0,
        Gradient = 1 << 1,
        Tint = 1 << 2,
        Shadow = 1 << 4,
        ShadowTint = 1 << 5,
        TextureGradient = Texture | Gradient,
        TextureTint = Texture | Tint,


    }

}