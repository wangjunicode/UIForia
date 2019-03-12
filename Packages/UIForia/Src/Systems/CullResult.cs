using System;

[Flags]
public enum CullResult {

    NotCulled,
    ClipRectIsZero,
    ActualSizeZero,
    OpacityZero,
    VisibilityHidden

}