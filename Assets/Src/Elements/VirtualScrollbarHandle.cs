using Src.Util;

namespace Src.Elements {

    public class VirtualScrollbarHandle : VirtualElement {

        public VirtualScrollbarHandle() {
            flags |= UIElementFlags.Enabled | UIElementFlags.AncestorEnabled;
            children = ArrayPool<UIElement>.Empty;
        }

    }

}