using System;

[Flags]
internal enum UIElementFlags {

    TextElement = 1 << 0,
    ImplicitElement = 1 << 1,
    Enabled = 1 << 2,
    AncestorEnabled = 1 << 3,
    Alive = 1 << 4,
    Primitive = 1 << 6,
    HasBeenEnabled = 1 << 7,
    Created = 1 << 8,
    VirtualElement = 1 << 9,
    TemplateRoot = 1 << 10,
    BuiltIn = 1 << 11,
    SelfAndAncestorEnabled = Alive | Enabled | AncestorEnabled,

    Registered = 1 << 14,

    DebugLayout = 1 << 15,

    EnabledThisFrame = 1 << 16,
    DisabledThisFrame = 1 << 17,


}