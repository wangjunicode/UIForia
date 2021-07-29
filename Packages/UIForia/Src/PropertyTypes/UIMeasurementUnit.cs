namespace UIForia.Layout {

    public enum UIMeasurementUnit : ushort {

        Unset = Unit.Unset,
        Pixel = Unit.Pixel,
        Content = Unit.Content,

        ViewportWidth = Unit.ViewportWidth,
        ViewportHeight = Unit.ViewportHeight,

        Em = Unit.Em,
        Percent = Unit.Percent,
        Stretch = Unit.Stretch,
        StretchContent = Unit.StretchContent, 
        FitContent = Unit.FitContent, 
        BackgroundImageWidth = Unit.BackgroundImageWidth,
        BackgroundImageHeight = Unit.BackgroundImageHeight,
        ApplicationWidth = Unit.ApplicationWidth,
        ApplicationHeight = Unit.ApplicationHeight,
        MaxChild = Unit.MaxChild, // stretch? 
        MinChild = Unit.MinChild, // stretch? 

        FillRemaining = Unit.FillRemaining,
        
        ParentSize = Unit.ParentSize,
        LineHeight = Unit.LineHeight

    }

}