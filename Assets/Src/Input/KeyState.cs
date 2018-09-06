using System;

namespace Src.Input {

    [Flags]
    public enum KeyState {

        Up = 0,
        Down = 1 << 0,
        DownThisFrame = Down | (1 << 2),
        UpThisFrame = Up | (1 << 3),

        UpNotThisFrame = Up,
        DownNotThisFrame = Down

    }

}