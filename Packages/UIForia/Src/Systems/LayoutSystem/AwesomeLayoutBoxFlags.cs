using System;

namespace UIForia.Systems {

    [Flags]
    public enum AwesomeLayoutBoxFlags {

        None = 0,
        HorizontalSizeChanged = 1,
        VerticalSizeChanged = 1 << 1,
        Ignored = 1 << 2,
        Transcluded = 1 << 3,
            
        RequireLayoutHorizontal = HorizontalSizeChanged,

        WidthBlockProvider = 1 << 4,
        HeightBlockProvider = 1 << 5,

        RequireLayoutVertical = VerticalSizeChanged,

        RequireAlignmentHorizontal = 1 << 6,
        RequireAlignmentVertical = 1 << 7

    }

}