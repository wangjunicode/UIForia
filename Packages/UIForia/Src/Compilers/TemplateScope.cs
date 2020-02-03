using System.Diagnostics;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct TemplateScope {

        public Application application;
        public StructList<SlotUsage> slotInputs;
        public UIElement innerSlotContext;
        public readonly bool retain;
        
        [DebuggerStepThrough]
        public TemplateScope(Application application, bool retain) {
            this.application = application;
            this.slotInputs = null;
            this.innerSlotContext = null;
            this.retain = retain;
        }

        public void AddSlotOverride(string slotName, UIElement context, int slotId) {
            slotInputs = slotInputs ?? StructList<SlotUsage>.Get();
            slotInputs.Add(new SlotUsage(slotName, slotId, context));
        }

        public void AddSlotForward(TemplateScope parentScope, string slotName, UIElement context, int slotId) {
            slotInputs = slotInputs ?? StructList<SlotUsage>.Get();
            if (parentScope.slotInputs == null) {
                slotInputs.Add(new SlotUsage(slotName, slotId, context));
                return;
            }
            else {
                for (int i = 0; i < parentScope.slotInputs.size; i++) {
                    if (parentScope.slotInputs.array[i].slotName == slotName) {
                        slotInputs.Add(parentScope.slotInputs.array[i]);
                        break;
                    }
                }
            }

            slotInputs.Add(new SlotUsage(slotName, slotId, context));
        }

        public void Release() {
            if (retain) {
                return;
            }
            slotInputs?.Release();
            application = null;
            innerSlotContext = null;
        }

    }

}