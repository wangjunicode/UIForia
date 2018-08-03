using System.Collections.Generic;

namespace Src {
    public class UIElementTemplate {
        public string text;
        public UIElementTemplate parent;
        public readonly ProcessedType type;
        public readonly List<UIElementTemplate> children;

        // input attributes
        public readonly List<PropertyBindPair> propBindings;

        public UIElementTemplate(ProcessedType type) {
            this.type = type;
            propBindings = new List<PropertyBindPair>();
            children = new List<UIElementTemplate>();
        }
    }
}