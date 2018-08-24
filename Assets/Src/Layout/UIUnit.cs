using System;

namespace Rendering {

    [Flags]
    public enum UIUnit {

        Pixel = 1 << 0,
        Content = 1 << 1,
        Parent = 1 << 2,
        View = 1 << 3,
        Auto = 1 << 4
               
        /*
         * Auto works as follows:
         *     It ignores the value provided
         *     If the dimension is width, it will fill the parent container but clamp to parent dimension
         *     If the dimension is height, it will grow to the size required to fill the content based on current width.
         *     
         */

    }

}