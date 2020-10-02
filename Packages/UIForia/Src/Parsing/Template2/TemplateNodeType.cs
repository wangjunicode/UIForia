using System;

namespace UIForia {

    [Flags]
    public enum TemplateNodeType {

        Unresolved = 0,
        SlotDefine = 1 << 0,
        SlotForward = 1 << 1,
        SlotOverride = 1 << 2,
        Root = 1 << 6,
        Text = 1 << 7,
        Meta = 1 << 9,
        Slot = SlotDefine | SlotForward | SlotOverride,

    }

}