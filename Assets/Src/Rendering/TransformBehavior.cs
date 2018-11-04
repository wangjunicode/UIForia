using System;

namespace Src.Rendering {

    [Flags]
    public enum TransformBehavior {

        Unset = 0,
        Default = 1 << 0, 
        LayoutOffset = 1 << 1,
        AnchorMinOffset = 1 << 2,
        AnchorMaxOffset = 1 << 3,
        PivotOffset = 1 << 4,

    }

}