using System;
using UIForia.Elements;

namespace UIForia.Compilers {

    public struct SlotOverride {

        public readonly string slotName;
        public readonly Func<ElementSystem, UIElement, UIElement> template;

        public SlotOverride(string slotName, Func<ElementSystem, UIElement, UIElement> template) {
            this.slotName = slotName;
            this.template = template;
        }

    }

}