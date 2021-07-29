using System;

namespace UIForia.Style {

    [Flags]
    // for pointer alignment reasons this should stay an integer
    public enum StyleState {

        Normal = 0,
        Hover = 1 << 1,
        Active = 1 << 2,
        Focus = 1 << 3,

    }

    [Flags]
    public enum StyleStateByte : byte {

        // note: order here is really important since other flag types use it
        Normal = 0,
        Hover = 1 << 1,
        Focused = 1 << 2,
        Active = 1 << 3

    }

}