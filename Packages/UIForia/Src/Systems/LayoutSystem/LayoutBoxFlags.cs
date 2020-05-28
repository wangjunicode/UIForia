using System;

namespace UIForia.Systems {

    [Flags]
    public enum LayoutBoxFlags {

        None = 0,
        RequireLayoutHorizontal = 1,
        RequireLayoutVertical = 1 << 1,
        Ignored = 1 << 2,
        AlwaysUpdate = 1 << 3,

        WidthBlockProvider = 1 << 4,
        HeightBlockProvider = 1 << 5

    }

}