using System;

namespace UIForia {

    [Flags]
    public enum StyleState2 : int {

        // note: order here is really important since other flag types use it
        Normal = 1 << 0,
        Hover = 1 << 1,
        Focused = 1 << 2,
        Active = 1 << 3

    }

    [Flags]
    public enum StyleState2UShort : ushort {

        // note: order here is really important since other flag types use it
        Normal = 1 << 0,
        Hover = 1 << 1,
        Focused = 1 << 2,
        Active = 1 << 3

    }

    [Flags]
    public enum StyleState2Byte : byte {

        // note: order here is really important since other flag types use it
        Normal = 1 << 0,
        Hover = 1 << 1,
        Focused = 1 << 2,
        Active = 1 << 3

    }

}