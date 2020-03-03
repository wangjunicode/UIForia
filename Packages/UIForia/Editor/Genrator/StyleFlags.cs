using System;

namespace UIForia.Editor {

    [Flags]
    public enum StyleFlags {

        Inherited = 1 << 0,
        Animated = 1 << 1,
        RequireGCHandleFree = 1 << 2

    }

}