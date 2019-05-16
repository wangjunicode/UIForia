using System;

namespace UIForia.Layout {

    [Flags]
    public enum UIMeasurementUnit {

        Unset = UnitConstants.Unset,
        Pixel = UnitConstants.Pixel,
        Content = UnitConstants.Content,
        ParentSize = UnitConstants.ParentSize,
        ViewportWidth = UnitConstants.ViewportWidth,
        ViewportHeight = UnitConstants.ViewportHeight,
        ParentContentArea = UnitConstants.ParentContentArea,
        Em = UnitConstants.Em,
        AnchorWidth = UnitConstants.AnchorWidth,
        AnchorHeight = UnitConstants.AnchorHeight,
        LineHeight = UnitConstants.LineHeight,
//        FractionalRemaining = UnitConstants.FractionalRemaining,

    }

}
