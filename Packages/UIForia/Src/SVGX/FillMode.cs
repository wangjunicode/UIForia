using System;

namespace SVGX {

    [Flags]
    public enum FillMode {

        Color = 0,
        Texture = 1 << 0,
        Gradient = 1 << 1,
        Pattern = 1 << 4,
        GradientAsTint = 1 << 5,
        
        TextureGradient = Texture | Gradient,
        TextureColor = Texture | Color

    }

}