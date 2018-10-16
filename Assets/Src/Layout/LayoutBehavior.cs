using System;

namespace Src.Layout {

    [Flags]
    public enum LayoutBehavior {

        Unset = 0,
        Normal = 1 << 0,
        Fixed = 1 << 1,
        Sticky = 1 << 2,
        Anchor = 1 << 3,
        Ignored = 1 << 4,

    }

}