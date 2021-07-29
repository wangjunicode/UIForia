using System;

namespace UIForia.Text {

    [Flags]
    internal enum CharacterRunFlags : byte {

        None = 0,
        Whitespace = 1 << 0,
        NewLine = 1 << 1,
        Tab = 1 << 2,
        HorizontalSpace = 1 << 3,
        Sprite = 1 << 4,
        LastInGroup = 1 << 5,
        Characters = 1 << 6,
        InlineSpace = 1 << 7,
        
        RequiresShaping = Characters,
    }

}