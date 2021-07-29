using System;

namespace UIForia.Layout {

    [Flags]
    internal enum SizeTypeFlags : byte {

        Resolved = 0, // all sizes are computable and we are ready for parent to run layout
        
        LayoutReady = 1 << 0, // parent laid us out already and we now have a final size
        LayoutComplete = 1 << 1, // we finished our layout and will do no more work this frame
        
        RequiresOppositeAxis = 1 << 2, // we depend on the other axis, either text or aspect lock
        RequiresContentValue = 1 << 3, // we need a value from one or more of our children before we can report a size
        RequiresParentSize = 1 << 4, // we need the size of our parent 
        RequiresStretchValue = 1 << 5, // we expect to stretch and haven't yet

        ContentReady = 1 << 6,
        
        HasFinalSize = LayoutReady | LayoutComplete,

        RequiresUpwardValue = RequiresParentSize | RequiresStretchValue,
        
        RequiresExternalValue = RequiresUpwardValue | RequiresContentValue,
        
        UnmetRequirement = RequiresOppositeAxis | RequiresExternalValue

    }

}