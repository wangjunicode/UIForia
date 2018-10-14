using System;

namespace Src.Layout.LayoutTypes {

    [Flags]
    public enum GridTemplateUnit {

        Unset = 0,
        Pixel = 1 << 0,
        Container = 1 << 1,
        ContainerContentArea = 1 << 2,
        Em = 1 << 3,
        ViewportWidth = 1 << 4,
        ViewportHeight = 1 << 5,

        Flex = 1 << 6,
        MinContent = 1 << 7,
        MaxContent = 1 << 8,

        Fixed = Pixel | Container | ContainerContentArea | Em | ViewportWidth | ViewportHeight

    }

}