using System;

namespace UIForia.Layout.LayoutTypes {

    [Flags]
    public enum GridTemplateUnit {

        Unset = UnitConstants.Unset,
        Pixel = UnitConstants.Pixel,
        ParentSize = UnitConstants.ParentSize,
        ParentContentArea = UnitConstants.ParentContentArea,
        Em = UnitConstants.Em,
        ViewportWidth = UnitConstants.ViewportWidth,
        ViewportHeight = UnitConstants.ViewportHeight,

        FractionalRemaining = UnitConstants.FractionalRemaining,
        MinContent = UnitConstants.MinContent,
        MaxContent = UnitConstants.MaxContent,
        Percent = UnitConstants.Percent,

        Fixed = Pixel | Percent | ParentSize | ParentContentArea | Em | ViewportWidth | ViewportHeight,


    }

}
