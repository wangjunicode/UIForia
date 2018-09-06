using System;

namespace Src.Input {

    [Flags]
    public enum KeyboardModifiers {

        None = 0,
        
        LeftAlt = 1 << 0,
        RightAlt = 1 << 1,
        Alt = LeftAlt | RightAlt,

        LeftShift = 1 << 2,
        RightShift = 1 << 3,
        Shift = LeftShift | RightShift,

        LeftControl = 1 << 4,
        RightControl = 1 << 5,
        Control = LeftControl | RightControl,

        LeftCommand = 1 << 6,
        RightCommand = 1 << 7,
        Command = LeftCommand | RightCommand,

        LeftWindows = 1 << 8,
        RightWindows = 1 << 9,
        Windows = LeftWindows | RightWindows,
        
        NumLock = 1 << 10,
        CapsLock = 1 << 11

    }

}