using System;

[Flags]
internal enum UIElementFlags {

    RequiresRendering = 1 << 0,
    TextElement = 1 << 1,
    ImplicitElement = 1 << 2,
    RequiresLayout = 1 << 3,
    Enabled = 1 << 4,
    Shown = 1 << 5,
    AncestorShown = 1 << 6,
    AncestorDisabled = 1 << 7

}