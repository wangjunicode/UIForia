﻿using System;

namespace UIForia {

    [Flags]
    public enum KeyboardModifiers : byte {

        None = 0,
        Alt = 1 << 0,
        Shift = 1 << 1,
        Control = 1 << 2,
        Command = 1 << 3,
        Windows = 1 << 4,
        NumLock = 1 << 5,
        CapsLock = 1 << 6

    }

}