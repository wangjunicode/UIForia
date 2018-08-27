using System;

// todo make internal but available to testing
[Flags]
public enum UIElementFlags {

    TextElement = 1 << 0,
    ImplicitElement = 1 << 1,
    
    RequiresLayout = 1 << 2,
    RequiresRendering = 1 << 3,
    
    Enabled = 1 << 4,
    AncestorEnabled = 1 << 5,
    
    Shown = 1 << 6,
    AncestorShown = 1 << 7,
    
    Destroyed = 1 << 8,
    AncestorDestroyed = 1 << 9,
    
    SelfAndAncestorShown = Shown | AncestorShown,
    SelfAndAncestorEnabled = Enabled | AncestorEnabled,

    PendingEnable = 1 << 10,
    PendingDisable = 1 << 11

}