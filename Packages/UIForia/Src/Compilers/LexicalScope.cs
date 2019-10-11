using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Compilers {

    public readonly struct LexicalScope {

        public readonly UIElement root;
        public readonly CompiledTemplate data;
        public readonly StructList<SlotUsage> slotInputList;
        
        public LexicalScope(UIElement root, CompiledTemplate data, StructList<SlotUsage> slotInputList) {
            this.root = root;
            this.data = data;
            this.slotInputList = slotInputList;
        }

    }

}