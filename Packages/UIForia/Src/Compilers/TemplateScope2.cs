using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Compilers {

    public readonly struct TemplateScope2 {

        public readonly Application application;
        public readonly StructList<SlotUsage> slotInputs;
        public readonly LinqBindingNode bindingNode;
        public readonly CompiledTemplate compiledTemplate;

        public TemplateScope2(Application application, LinqBindingNode bindingNode, StructList<SlotUsage> slotInputs) {
            this.application = application;
            this.slotInputs = slotInputs;
            this.bindingNode = bindingNode;
            this.compiledTemplate = null;
        }

    }

}