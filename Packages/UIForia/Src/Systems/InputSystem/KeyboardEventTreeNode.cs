using System.Collections.Generic;
using UIForia.Input;

namespace UIForia.Systems {

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