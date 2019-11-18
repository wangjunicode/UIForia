using System;

[Flags]
internal enum UIElementFlags {

    // Element Flags
    ImplicitElement = 1,
    Enabled = 1 << 1,
    AncestorEnabled = 1 << 2,
    Alive = 1 << 3,
    HasBeenEnabled = 1 << 4,
    Primitive = 1 << 5,
    Created = 1 << 6, // can maybe get rid fo this when revisiting 
    TemplateRoot = 1 << 7,
    
    SelfAndAncestorEnabled = Alive | Enabled | AncestorEnabled,
    
    // Layout Flags
    DebugLayout = 1 << 16,
    LayoutSizeWidthDirty = 1 << 17,
    LayoutSizeHeightDirty = 1 << 18,
    LayoutHierarchyDirty = 1 << 19,
    LayoutTransformNotIdentity = 1 << 20,
    LayoutTransformDirty = 1 << 21,
    LayoutAlignmentHorizontalDirty = 1 << 22,
    LayoutAlignmentVerticalDirty = 1 << 23,
    LayoutFitWidthDirty = 1 << 24,
    LayoutFitHeightDirty = 1 << 25,
    LayoutTypeOrBehaviorDirty = 1 << 26,
    LayoutBorderPaddingHorizontalDirty = 1 << 27,
    LayoutBorderPaddingVerticalDirty = 1 << 28,

    AliveEnabledAncestorEnabled = Alive | Enabled | AncestorEnabled,
    
    DefaultLayoutDirty = (
        LayoutSizeWidthDirty |
        LayoutSizeHeightDirty |
        LayoutTypeOrBehaviorDirty 
    ),

    LayoutFlags = (
        LayoutSizeWidthDirty |
        LayoutSizeHeightDirty |
        LayoutHierarchyDirty |
        LayoutTransformNotIdentity |
        LayoutAlignmentHorizontalDirty |
        LayoutAlignmentVerticalDirty |
        LayoutFitWidthDirty |
        LayoutFitHeightDirty |
        LayoutTypeOrBehaviorDirty |
        LayoutBorderPaddingHorizontalDirty |
        LayoutBorderPaddingVerticalDirty
    )

}