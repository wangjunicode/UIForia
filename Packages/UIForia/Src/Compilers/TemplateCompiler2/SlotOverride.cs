using System;

namespace UIForia.Compilers {

    public struct SlotOverride {

        public readonly string slotName;
        public readonly Action<ElementSystem> template;

        public SlotOverride(string slotName, Action<ElementSystem> template) {
            this.slotName = slotName;
            this.template = template;
        }

    }

}