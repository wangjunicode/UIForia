using System;

namespace Src.Input {

    [Flags]
    public enum InputEventType {

        MouseEnter = 1 << 0,
        MouseExit = 1 << 1,
        MouseUp = 1 << 2,
        MouseDown = 1 << 3,
        MouseMove = 1 << 4,
        MouseHover = 1 << 5,
        MouseContext = 1 << 6,
        MouseScroll = 1 << 7

    }

}