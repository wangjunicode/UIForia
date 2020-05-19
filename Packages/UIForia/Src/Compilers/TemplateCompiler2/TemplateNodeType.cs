using System;

namespace UIForia.Compilers {

    [Flags]
    public enum TemplateNodeType {

        Unresolved = 0,
        SlotDefine = 1 << 0,
        SlotForward = 1 << 1,
        SlotOverride = 1 << 2,
        Container = 1 << 3,
        Expanded = 1 << 4,

        Root = 1 << 6, 
        Text = 1 << 7,
        Repeat = 1 << 8,
        TextSpan = 1 << 9,

        Slot = SlotDefine | SlotForward | SlotOverride,



    }

}