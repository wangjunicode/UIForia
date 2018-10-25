using Src.Util;

namespace Src.Elements {

    public class VirtualScrollbarHandle : VirtualElement {

        public VirtualScrollbarHandle() {
            flags |= UIElementFlags.Enabled | UIElementFlags.AncestorEnabled;
            ownChildren = ArrayPool<UIElement>.Empty;
        }

    }

}