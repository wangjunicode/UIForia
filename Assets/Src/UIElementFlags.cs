using System;

[Flags]
internal enum UIElementFlags {

    RequiresRendering = 1    << 0,
    HasRenderedElement = 1   << 1,
    RenderElementCreated = 1 << 2,
    TextElement = 1 << 3,
    ImplicitElement = 1 << 4,

}