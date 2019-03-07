using System;

namespace UIForia.Rendering {

    [Flags]
    public enum StyleState {

        // todo -- reorganize by priority since this is a sort key
        Normal = 1 << 0,
        Active = 1 << 1,
        Hover = 1 << 2,
        Focused = 1 << 3,

    }

}