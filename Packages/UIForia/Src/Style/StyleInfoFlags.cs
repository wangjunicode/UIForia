using System;

namespace UIForia.Style {

    [Flags]
    // for pointer alignment reasons this should stay an integer

    internal enum StyleInfoFlags {

        AllocatedStyleList = 1 << 0,
        InitThisFrame = 1 << 1,
        RequirePropertyBuild = 1 << 2,
        StyleListUpdated = 1 << 3,
        InstanceStyleChanges = 1 << 4,
        AnimationStyleChanged = 1 << 5,
        
        StyledStateChanged = 1 << 6, 
        RequireUpdateOnAttributeSet = 1 << 7,
        RequireUpdateOnSiblingIndexChange = 1 << 8,
        RequireUpdateOnChildCountChange = 1 << 9,

        TransitionStyleChanged = 1 << 10,
        
        RequireWhenEval = 1 << 11,
        
        
        WhenFlags = StyledStateChanged | RequireUpdateOnAttributeSet | RequireUpdateOnSiblingIndexChange | RequireUpdateOnChildCountChange,
        PerFrameFlags = InitThisFrame | RequirePropertyBuild | StyleListUpdated | InstanceStyleChanges | AnimationStyleChanged | StyledStateChanged | RequireWhenEval,


    }

}