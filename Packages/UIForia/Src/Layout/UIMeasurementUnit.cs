using System;

namespace UIForia.Rendering {

    [Flags]
    public enum UIMeasurementUnit {

        Unset = 0,
        Pixel = 1 << 0,
        Content = 1 << 1,
        ParentSize = 1 << 2,
        ViewportWidth = 1 << 3,
        ViewportHeight = 1 << 4,
        ParentContentArea = 1 << 5,
        Em = 1 << 6,
        AnchorWidth = 1 << 7,
        AnchorHeight = 1 << 8,
        LineHeight = 1 << 9,
        MaxContentSelf = 1 << 10,
        MinContentSelf = 1 << 11

    }

}