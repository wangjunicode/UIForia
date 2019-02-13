using System;

namespace UIForia.Text {

    [Flags]
    public enum FontStyle {

        Unset = 0,
        Normal = 1 << 1,
        Bold = 1 << 2,
        Italic = 1 << 3,
        Underline = 1 << 4,
        LowerCase = 1 << 5,
        UpperCase = 1 << 6,
        SmallCaps = 1 << 7,
        StrikeThrough = 1 << 8,
        Superscript = 1 << 9,
        Subscript = 1 << 10,
        Highlight = 1 << 11,

    }

}