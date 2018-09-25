using System;

namespace Src.Layout {

    [Flags]
    public enum GridTrackSizeType {

        Fractional = 1 << 0,
        Pixel = 1 << 1,
        Em = 1 << 2,
        MaxContent = 1 << 3,
        MinContent = 1 << 4,
        FitContent = 1 << 5,
        Viewport = 1 << 7,
        Parent = 1 << 8,
        Content = 1 << 9,
        Auto = 1 << 10,

        Intrinsic = Auto | MinContent | MaxContent | FitContent

    }

}