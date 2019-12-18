using System.Diagnostics;
using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Compilers {

    public readonly struct TemplateScope {

        public readonly Application application;
        public readonly StructList<SlotUsage> slotInputs;

        [DebuggerStepThrough]
        public TemplateScope(Application application, StructList<SlotUsage> slotInputs) {
            this.application = application;
            this.slotInputs = slotInputs;
        }

        public void ForwardSlotUsage(string slotName, StructList<SlotUsage> slotList) {
            for (int i = 0; i < slotInputs.size; i++) {
                if (slotInputs.array[i].slotName == slotName) {
                    slotList.Add(slotInputs.array[i]);
                    return;
                }
            }
        }
        
          public void ForwardSlotUsageWithFallback(string slotName, StructList<SlotUsage> slotList, UIElement fallbackContext, int fallbackId) {
              if (slotInputs != null) {
                  for (int i = 0; i < slotInputs.size; i++) {
                      if (slotInputs.array[i].slotName == slotName) {
                          slotList.Add(slotInputs.array[i]);
                          return;
                      }
                  }
              }

              slotList.Add(new SlotUsage(slotName, fallbackId, fallbackContext));
        }

    }

}