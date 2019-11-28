using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Compilers {

    public readonly struct TemplateScope2 {

        public readonly Application application;
        public readonly StructList<SlotUsage> slotInputs;

        public TemplateScope2(Application application, StructList<SlotUsage> slotInputs) {
            this.application = application;
            this.slotInputs = slotInputs;
        }

    }

}