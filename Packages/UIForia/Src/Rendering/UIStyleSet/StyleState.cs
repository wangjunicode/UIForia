using System;

namespace UIForia.Rendering {

    [Flags]
    public enum StyleType {

        /// Styles applied to the element type, overrides the built-in default styles
        Implicit = 1 << 1,

        /// Regular style applied to one or more elements, overrides implicit styles
        Shared = 1 << 2,

        /// Set only on one element, like inline styles, overrides all other styles
        Instance = 1 << 3

    }

    [Flags]
    public enum StyleState {

        // todo -- reorganize by priority since this is a sort key
        Normal = 1 << 0,
        Active = 1 << 1,
        Hover = 1 << 2,
        Focused = 1 << 3,

    }

}