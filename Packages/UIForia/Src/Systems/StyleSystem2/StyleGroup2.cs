using UIForia.Style;

namespace UIForia {

    public struct StyleGroup2 {

        public StyleId id;
        public int styleSheetId;
        
        public int normalRange;
        public int activeRange;
        public int focusRange;
        public int hoverRange;
        
        public StyleProperty2[] properties;

        public int normalSelectorRange;
        public int activeSelectorRange;
        public int focusSelectorRange;
        public int hoverSelectorRange;

        //todo  arrays could also be stored as 1 array with ranges
        public StyleProperty2[] normalProperties;
        public StyleProperty2[] hoverProperties;
        public StyleProperty2[] focusProperties;
        public StyleProperty2[] activeProperties;

        // maybe 1 array is better for all states and selector hooks?
        public StyleRunCommandSet normalRunCommands;
        public StyleRunCommandSet hoverRunCommands;
        public StyleRunCommandSet focusRunCommands;
        public StyleRunCommandSet activeRunCommands;

        public SelectorDefinition[] selectorDefinitions;

    }

}