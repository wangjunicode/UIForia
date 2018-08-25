using System;

[Flags]
internal enum UIElementFlags {

    RequiresRendering = 1    << 0,
    HasRenderedElement = 1   << 1,
    RenderElementCreated = 1 << 2,
    TextElement = 1 << 3,
    ImplicitElement = 1 << 4,
    RequiresLayout = 1 << 5,
    
    HandlesMouseDown = 1 << 6,
    HandlesMouseUp = 1 << 7,
    HandlesMouseMove = 1 << 8,
    HandlesMouseEnter = 1 << 9,
    HandlesMouseExit = 1 << 10,
    HandlesMouseContext = 1 << 11,
    HandlesMouseClick = 1 << 12,
    HandlesMouseScroll = 1 << 13,
    HandlesDragStart = 1 << 14,
    HandlesDragEnd = 1 << 15,
    HandlesDragUpdate = 1 << 16,
    HandlesDragCancel = 1 << 17,
    HandlesDragDrop = 1 << 18

}