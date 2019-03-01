using UIForia.Elements;
using UIForia.UIInput;
using UIForia.Util;

namespace UIForia.Stystems.InputSystem {

    public class KeyboardEventTreeNode : IHierarchical {

        private readonly UIElement element;
        public readonly KeyboardEventHandler[] handlers;

        public KeyboardEventTreeNode(UIElement element, KeyboardEventHandler[] handlers) {
            this.element = element;
            this.handlers = handlers;
        }

        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;

    }
}