using System;

namespace UIForia.Text {

    [Flags]
    public enum WhitespaceMode {

        CollapseWhitespace = 1 << 0,
        PreserveNewLines = 1 << 1,
        TrimStart = 1 << 2,
        TrimEnd = 1 << 3,
        Trim = TrimStart | TrimEnd,

        TrimLineStart = 1 << 4 //allow lines to begin with whitespace

    }

}