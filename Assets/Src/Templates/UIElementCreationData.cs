using System.Collections.Generic;
using Rendering;
using Src.InputBindings;
using Src.StyleBindings;

namespace Src {

    public class UIElementCreationData {

        public UIElement element;
        public Binding[] bindings;
        public InputBinding[] inputBindings;
        public Binding[] constantBindings;
        public List<UIStyle> baseStyles;
        public List<StyleBinding> constantStyleBindings;
        
        public UITemplateContext context;

        public int elementId => element.id;
        public string name => element.name;
        
        public int GetSiblingIndex() {
            return 0;
        }

        public int GetDepth() {
            int depth = 0;
            UIElement ptr = element;
            while (ptr != null) {
                ptr = ptr.parent;
                depth++;
            }

            return depth;
        }

    }

}