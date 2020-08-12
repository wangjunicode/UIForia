using System;

namespace UIForia.Layout {

    [Flags]
    public enum UIMeasurementUnit {

        Unset = UnitConstants.Unset,
        Auto = UnitConstants.Auto,
        Pixel = UnitConstants.Pixel,
        Content = UnitConstants.Content,
        BlockSize = UnitConstants.ParentSize,
        ViewportWidth = UnitConstants.ViewportWidth,
        ViewportHeight = UnitConstants.ViewportHeight,
        ParentContentArea = UnitConstants.ParentContentArea,
        Em = UnitConstants.Em,
        Percentage = UnitConstants.Percent,
        IntrinsicMinimum = UnitConstants.MinContent,
        IntrinsicPreferred = UnitConstants.MaxContent,
        FitContent = UnitConstants.FitContent,
        
        BackgroundImageWidth = UnitConstants.TextureWidth,
        BackgroundImageHeight = UnitConstants.TextureHeight,

    }

}
