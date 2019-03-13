using System;

namespace UIForia.Layout.LayoutTypes {

    [Flags]
    public enum GridTemplateUnit {

        Unset = 0,
        Pixel = 1 << 0,
        ParentSize = 1 << 1,
        ParentContentArea = 1 << 2,
        Em = 1 << 3,
        ViewportWidth = 1 << 4,
        ViewportHeight = 1 << 5,

        FractionalRemaining = 1 << 6,
        MinContent = 1 << 7,
        MaxContent = 1 << 8,

        Fixed = Pixel | ParentSize | ParentContentArea | Em | ViewportWidth | ViewportHeight

    }

}