using System;

// todo make internal but available to testing
[Flags]
public enum UIElementFlags {

    TextElement = 1 << 0,
    ImplicitElement = 1 << 1,
    
//    RequiresLayout = 1 << 2,
    RequiresRendering = 1 << 3,
    
    Enabled = 1 << 4,
    AncestorEnabled = 1 << 5,
    
    Shown = 1 << 6,
    AncestorShown = 1 << 7,
    
    Destroyed = 1 << 8,
    AncestorDestroyed = 1 << 9,
    
    RequiresSpecialRendering = 1 << 10,
    
    Primitive = 1 << 11,
    
    AcceptFocus = 1 << 12,
    
    IsFocused = 1 << 13,
    Image = 1 << 14,
    TextContainer = 1 << 15,
    
    Reparentable = 1 << 16,
    Initialized = 1 << 17,
    Created = 1 << 18,
    VirtualElement = 1 << 19,
    TemplateRoot = 1 << 20,
    SelfAndAncestorShown = Shown | AncestorShown,
    SelfAndAncestorEnabled = Enabled | AncestorEnabled,



}