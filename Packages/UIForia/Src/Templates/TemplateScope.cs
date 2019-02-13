using System.Collections.Generic;
using UIForia.Util;

namespace UIForia {

    public struct TemplateScope {

        public readonly UIElement rootElement;
        public List<UISlotContentTemplate> slotContents;
        
        public TemplateScope(UIElement rootElement) {
            this.rootElement = rootElement;
            this.slotContents = null;
        }

        public UISlotContentTemplate FindSlotContent(string slotName) {
            if (slotContents == null) return null;
            for (int i = 0; i < slotContents.Count; i++) {
                if (slotContents[i].SlotName == slotName) {
                    return slotContents[i];
                }
            }
            return null;
        }

    }

}