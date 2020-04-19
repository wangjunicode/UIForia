using System;

namespace UIForia {

    [Flags]
    public enum StyleEventFlags : ushort {

        HasEnterEvents = 1 << 4,
        HasExitEvents = 1 << 5,
        Mask = HasEnterEvents | HasExitEvents

    }

}