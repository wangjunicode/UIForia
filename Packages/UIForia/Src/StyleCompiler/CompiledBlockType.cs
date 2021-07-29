using System;

namespace UIForia.Compilers {

    [Flags]
    internal enum CompiledBlockType {

        Root = 1 << 0,
        State = 1 << 1,
        Attribute = 1 << 2,


    }

}