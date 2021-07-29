using System;

namespace UIForia.Text {

    [Flags]
    public enum WhitespaceMode : byte {

        None = 0,
        CollapseWhitespace = 1 << 0,
        PreserveNewLines = 1 << 1,
        TrimStart = 1 << 2,
        TrimEnd = 1 << 3,
        
        TrimLineStart = 1 << 4, //allow lines to begin with whitespace
        TrimLineEnd = 1 << 5, //allow lines to end with whitespace
        
        NoWrap = 1 << 6,
        CollapseMultipleNewLines = 1 << 7,

        TrimLine = TrimLineStart | TrimLineEnd,
        Trim = TrimStart | TrimEnd,
        CollapseWhitespaceAndTrim = Trim | CollapseWhitespace,

    }

}