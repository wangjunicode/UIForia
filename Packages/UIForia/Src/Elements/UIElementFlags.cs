using System;

[Flags]
internal enum UIElementFlags {

    TextElement = 1 << 0,
    ImplicitElement = 1 << 1,
    Enabled = 1 << 2,
    AncestorEnabled = 1 << 3,
    Destroyed = 1 << 4,
    AncestorDestroyed = 1 << 5,
    Primitive = 1 << 6,
    HasBeenEnabled = 1 << 7,
    Created = 1 << 8,
    VirtualElement = 1 << 9,
    TemplateRoot = 1 << 10,
    BuiltIn = 1 << 11,
    CreatedExplicitly = 1 << 12, // was this element created via user code?
    SelfAndAncestorEnabled = Enabled | AncestorEnabled,

    Ready = 1 << 13,
    
    Registered = 1 << 14

}