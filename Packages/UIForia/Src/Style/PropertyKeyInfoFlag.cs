using System;

namespace UIForia.Style {

    [Flags]
    public enum PropertyKeyInfoFlag {

        ContainsConst = 1 << 1,
        ContainsVariable = 1 << 2,
        ContainsMixinVariable = 1 << 3,
        ExplicitDefault = 1 << 4,
        ExplicitInherit = 1 << 5,
        AnimationCurrent = 1 << 6,
        Transitioned = 1 << 7

    }

}