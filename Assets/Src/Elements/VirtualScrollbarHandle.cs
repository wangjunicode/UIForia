using UIForia.Util;

namespace UIForia.Elements {

    public class VirtualScrollbarHandle : VirtualElement {

        public VirtualScrollbarHandle() {
            flags |= UIElementFlags.Enabled | UIElementFlags.AncestorEnabled;
            children = ArrayPool<UIElement>.Empty;
        }

    }

}