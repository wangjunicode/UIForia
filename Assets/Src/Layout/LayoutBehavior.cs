using System;

namespace Src.Layout {

    [Flags]
    public enum LayoutBehavior {

        Unset = 0,
        Normal = 1 << 0,
        Anchors = 1 << 1,
        Ignored = 1 << 2,

    }

}