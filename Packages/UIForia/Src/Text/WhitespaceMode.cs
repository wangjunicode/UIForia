using System;

namespace UIForia.Text {

    [Flags]
    public enum WhitespaceMode {

        CollapseWhitespace = 1 << 0,
        PreserveNewLines = 1 << 1

    }

}