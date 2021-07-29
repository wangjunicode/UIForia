using System;

namespace UIForia.Style {

    /// <summary>
    /// none        default value. Animation will not apply any styles to the element before or after it is executing
    /// forwards	The element will retain the style values that is set by the last keyframe (depends on animation-direction and animation-iteration-count)
    /// backwards	The element will get the style values that is set by the first keyframe (depends on animation-direction), and retain this during the animation-delay period
    /// both	    The animation will follow the rules for both forwards and backwards, extending the animation properties in both directions
    /// </summary>
    [Flags]
    public enum AnimationFillMode {

        None = 0,
        Forward = 1 << 0,
        Backwards = 1 << 1,
        Both = Forward | Backwards

    }

}