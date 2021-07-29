using System;

namespace UIForia.Layout {

    [Flags]
    internal enum ReadyFlags : byte { 

        ContentReady = 1 << 0,
        AspectReady = 1 << 1,
        ParentReady = 1 << 2,
        StretchReady = 1 << 3,
        LayoutComplete = 1 << 4,
        BaseSizeResolved = 1 << 5,
        FinalSizeResolved = 1 << 6,
        ContentSizeResolved = 1 << 7
        
    }

}