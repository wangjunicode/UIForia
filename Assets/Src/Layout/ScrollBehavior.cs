using System;

namespace Src.Layout {

    [Flags]
    public enum ScrollBehavior {

        Unset = 0,
        Normal = 1 << 0, // takes normal position
        Fixed = 1 << 1, // takes normal position - parent scroll offset
        Sticky = 1 << 2 // idea: sticky range ->
                        // become sticky at range start
                        // stop being sticky at range end

    }

}