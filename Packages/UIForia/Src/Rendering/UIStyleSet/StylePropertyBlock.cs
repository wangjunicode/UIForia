using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Style {

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

    public struct Style {

        public ushort index;
        public ushort styleSheetId;
        
        public ushort propertyStart;
        public ushort commandStart;

        private byte propertyCount;
        private byte commandCount;

        // has commands
        // has selectors
        // commandStart byte count
        // selectorStart byte count

    }

}