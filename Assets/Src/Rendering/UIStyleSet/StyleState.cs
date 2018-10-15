using System;

namespace Rendering {

    [Flags]
    public enum StyleType {

        Default = 1 << 0,
        Shared = 1 << 1,
        Instance = 1 << 2

    }

    [Flags]
    public enum StyleState {

        // todo -- reorganize by priority since this is a sort key
        Normal = 1 << 0,
        Hover = 1 << 1,
        Active = 1 << 2,
        Disabled = 1 << 3,
        Focused = 1 << 4,
        Animation = 1 << 5,

    }

}