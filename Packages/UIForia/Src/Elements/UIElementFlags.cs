using System;

namespace UIForia.Elements {

    [Flags]
    public enum UIElementFlags : ushort {

        None = 0,

        ViewRoot = 1 << 0,
        Enabled = 1 << 1,            // DO NOT MOVE THIS INDEX
        InitThisFrame = 1 << 2,      // DO NOT MOVE THIS INDEX 
        HasBeenEnabled = 1 << 3,
        TemplateRoot = 1 << 4,
        StyleListChanged = 1 << 5,
        RequireDisableInvoke = 1 << 6,
        HasCustomInputHandling = 1 << 7

    }

}