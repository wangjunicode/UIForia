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
        MouseScroll = 1 << 7,

        KeyDown = 1 << 8,
        KeyUp = 1 << 9,

        Focus = 1 << 10,
        Blur = 1 << 11,

        DragCreate = 1 << 12,
        DragMove = 1 << 13,
        DragHover = 1 << 14,
        DragEnter = 1 << 15,
        DragExit = 1 << 16,
        DragDrop = 1 << 17,
        DragCancel = 1 << 18,

        DragUpdate = DragMove | DragHover,
        MouseUpdate = MouseMove | MouseHover,

    }

}