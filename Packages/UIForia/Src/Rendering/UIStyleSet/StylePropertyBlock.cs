using UIForia.Rendering;
using UIForia.Selectors;

namespace UIForia {

    public struct StylePropertyBlock {

        public StyleProperty[] properties;
        public RunCommand[] runCommands;

        public StylePropertyBlock(StyleProperty[] properties, RunCommand[] runCommands = null) {
            this.properties = properties;
            this.runCommands = runCommands;
        }

    }
    
}