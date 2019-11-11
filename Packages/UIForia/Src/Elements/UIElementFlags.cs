using System;
using UIForia.Layout;

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

    EnabledThisFrame = 1 << 16,
    DisabledThisFrame = 1 << 17,

    // Layout Flags
    DebugLayout = 1 << 15,
    LayoutSizeWidthDirty = 1 << 18,
    LayoutSizeHeightDirty = 1 << 19,
    LayoutBoxModelWidthDirty = 1 << 20,
    LayoutBoxModelHeightDirty = 1 << 21,
    LayoutHierarchyDirty = 1 << 22,
    LayoutTransformDirty = 1 << 23,
    LayoutAlignmentHorizontalDirty = 1 << 24,
    LayoutAlignmentVerticalDirty = 1 << 25,
    LayoutFitWidthDirty = 1 << 26,
    LayoutFitHeightDirty = 1 << 27,
    LayoutTypeDirty = 1 << 28,
    LayoutBehaviorDirty = 1 << 29,
    LayoutBorderPaddingHorizontalDirty = 1 << 30,
    LayoutBorderPaddingVerticalDirty = 1 << 31,

    DefaultLayoutDirty = (
        LayoutSizeWidthDirty |
        LayoutSizeHeightDirty |
        LayoutBoxModelWidthDirty |
        LayoutBoxModelHeightDirty |
        LayoutTypeDirty |
        LayoutBehaviorDirty
    ),

    LayoutFlags = (
        LayoutSizeWidthDirty |
        LayoutSizeHeightDirty |
        LayoutBoxModelWidthDirty |
        LayoutBoxModelHeightDirty |
        LayoutHierarchyDirty |
        LayoutTransformDirty |
        LayoutAlignmentHorizontalDirty |
        LayoutAlignmentVerticalDirty |
        LayoutFitWidthDirty |
        LayoutFitHeightDirty |
        LayoutTypeDirty |
        LayoutBehaviorDirty |
        LayoutBorderPaddingHorizontalDirty |
        LayoutBorderPaddingVerticalDirty
    )

}