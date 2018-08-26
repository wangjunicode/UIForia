using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src.InputBindings;
using Src.StyleBindings;

namespace Src {

    [DebuggerDisplay("{element} depth: {depth}")]
    public class UIElementCreationData {

        public UIElement element;
        public Binding[] bindings;
        public InputBinding[] inputBindings;
        public Binding[] constantBindings;
        public List<UIStyle> baseStyles;
        public List<StyleBinding> constantStyleBindings;
        public Binding[] conditionalBindings;
        public UITemplateContext context;

        public int elementId => element.id;
        public string name => element.name;
        
        private int depth = int.MinValue;
        public List<UIElementCreationData> children = new List<UIElementCreationData>();

        public int GetDepth() {
            if (depth != int.MinValue) {
                return depth;
            }
            depth = 0;
            UIElement ptr = element.parent;
            while (ptr != null) {
                ptr = ptr.parent;
                depth++;
            }

            return depth;
        }

    }

}