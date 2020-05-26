using System;

namespace UIForia.Elements {

    [Flags]
    internal enum UIElementFlags {

        // Element Flags
        // 1 << 0 is free
        
        Enabled = 1 << 1,
        AncestorEnabled = 1 << 2,
        Alive = 1 << 3,
        HasBeenEnabled = 1 << 4,
        
        // 5 is free
        
        Created = 1 << 6, // can maybe get rid fo this when revisiting 
        TemplateRoot = 1 << 7,
        
        // 8 is free
        
        NeedsUpdate = 1 << 9,
        // Layout Flags
        DebugLayout = 1 << 10,
        LayoutHierarchyDirty = 1 << 11,
        LayoutTransformNotIdentity = 1 << 12,
        LayoutFitWidthDirty = 1 << 13,
        LayoutFitHeightDirty = 1 << 14,
        LayoutTypeOrBehaviorDirty = 1 << 15,

        EnabledFlagSet = Alive | Enabled | AncestorEnabled,
        EnabledFlagSetWithUpdate = EnabledFlagSet | NeedsUpdate,
        SelfAndAncestorEnabled = Alive | Enabled | AncestorEnabled,

    }

}