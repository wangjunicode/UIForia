using System.Diagnostics;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Compilers {

    public readonly struct TemplateScope {

        public readonly Application application;
        public readonly StructList<SlotUsage> slotInputs;
        public readonly UIElement slotUsageContext;
        
        [DebuggerStepThrough]
        public TemplateScope(Application application, StructList<SlotUsage> slotInputs, UIElement slotUsageContext) {
            this.application = application;
            this.slotInputs = slotInputs;
            this.slotUsageContext = slotUsageContext;
        }

    }

}