using System;

namespace UIForia {

    [Flags]
    public enum SelectorQueryScope {

        Children = 1 << 0,
        Descendents = 1 << 1,
        Template = 1 << 2

    }

}