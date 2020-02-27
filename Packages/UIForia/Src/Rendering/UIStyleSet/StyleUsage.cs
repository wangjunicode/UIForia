using System.Diagnostics;
using UIForia.Rendering;

namespace UIForia {

    [DebuggerDisplay("{property.propertyId} | {priority.state}")]
    public struct StyleUsage {

        public StyleSourceId sourceId;
        public StylePriority priority;
        public StyleProperty property;

    }

}