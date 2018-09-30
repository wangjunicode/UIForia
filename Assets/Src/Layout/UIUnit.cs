using System;

namespace Rendering {

    [Flags]
    public enum UIUnit {

        Unset = 0,
        Pixel = 1 << 0,
        Content = 1 << 1,
        ParentSize = 1 << 2,
        View = 1 << 3,
        ParentContentArea = 1 << 4,
        Em = 1 << 5,
        MinContent = 1 << 6, // max content size for track
        MaxContent = 1 << 7, // min content size for track
        FitContent = 1 << 8, // fit content size for track

        ConstFixed = Pixel | Em
                     
        /*
         * Auto works as follows:
         *     It ignores the value provided
         *     If the dimension is width, it will fill the parent container but clamp to parent dimension
         *     If the dimension is height, it will grow to the size required to fill the content based on current width.
         *     
         */,


    }

}