using System;

namespace UIForia.Rendering {

    [Flags]
    public enum TransformBehavior {

        Unset = 0,
        LayoutOffset = 1 << 1,
        AnchorMinOffset = 1 << 2,
        AnchorMaxOffset = 1 << 3,
        PivotOffset = 1 << 4,

    }

}