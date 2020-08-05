using System;

namespace UIForia.Elements {

    [Flags]
    public enum UIElementFlags : ushort {

        // Element Flags
        EnableStateChanged = 1 << 0,

        Enabled = 1 << 1,
        AncestorEnabled = 1 << 2,
        Alive = 1 << 3,
        HasBeenEnabled = 1 << 4,

        EnabledRoot = 1 << 5,
        DisableRoot = 1 << 6,

        Created = 1 << 7,
        TemplateRoot = 1 << 8,

        NeedsUpdate = 1 << 9,

        // Layout Flags, get rid of these
        DebugLayout = 1 << 10,

        EnabledFlagSet = Alive | Enabled | AncestorEnabled,
        EnabledFlagSetWithUpdate = EnabledFlagSet | NeedsUpdate,

    }

}