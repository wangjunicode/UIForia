using UIForia.Rendering;
using UnityEngine;

namespace UIForia {

    public struct StylePropertyBlock {

        public StyleProperty[] properties;
        public RunCommand[] runCommands;

        public StylePropertyBlock(StyleProperty[] properties, RunCommand[] runCommands = null) {
            this.properties = properties;
            this.runCommands = runCommands;
        }

    }

    public struct StyleGrouping {

        public RangeInt normal;
        public RangeInt hover;
        public RangeInt active;
        public RangeInt focus;
        public Range16 normalCommands;
        public Range16 focusCommands;
        public Range16 hoverCommands;
        public Range16 activeCommands;

    }

}