using System;

namespace UIForia.Style {

    [Flags]
    public enum PropertyTypeFlags : byte {

        None = 0,
        Inherited = 1 << 0,
        Animated = 1 << 1, // not sure we need this since we'll just have a switch somewhere for logic anyway, but maybe doesnt hurt, useful for transitions
        RequireDestruction = 1 << 2,
        Contextual = 1 << 3
    }

}