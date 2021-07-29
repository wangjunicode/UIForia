
namespace UIForia.Layout.LayoutTypes {

    public enum GridTemplateUnit : byte {

        Unset = Unit.Unset,
        Pixel = Unit.Pixel,
        Em = Unit.Em,
        
        ViewportWidth = Unit.ViewportWidth,
        ViewportHeight = Unit.ViewportHeight,

        MinContent = Unit.MinContent,
        MaxContent = Unit.MaxContent,
        Percent = Unit.Percent,
        FillRemaining = Unit.FillRemaining,
        Stretch = Unit.Stretch,

        ApplicationWidth = Unit.ApplicationWidth,
        ApplicationHeight = Unit.ApplicationHeight

    }

}
