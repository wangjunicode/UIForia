using System;

namespace Rendering {

    [Flags]
    public enum StyleStateType {

        // todo -- reorganize by priority since this is a sort key
        Normal = 1   << 0,
        Hover = 1    << 1,
        Active = 1   << 2,
        Disabled = 1 << 3,
        Focused = 1  << 4,

        Instance = 1 << 5,
        Base = 1     << 6,

        IsState = Hover | Active | Disabled | Focused,
        InstanceNormal = Instance           | Normal,
        InstanceHover = Instance            | Hover,
        InstanceActive = Instance           | Active,
        InstanceDisabled = Instance         | Disabled,
        InstanceFocused = Instance          | Focused,

        BaseNormal = Base   | Normal,
        BaseHover = Base    | Hover,
        BaseActive = Base   | Active,
        BaseDisabled = Base | Disabled,
        BaseFocused = Base  | Focused,

    }

}