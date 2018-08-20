using System;

namespace Rendering {

    [Flags]
    public enum UIUnit {

        Pixel = 1 << 0,
        Content = 1 << 1,
        Parent = 1 << 2,
        View = 1 << 3,

    }

}