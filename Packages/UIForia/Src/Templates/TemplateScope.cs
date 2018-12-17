using UIForia.Util;

namespace UIForia {

    public struct TemplateScope {

        public readonly UIElement rootElement;
        public LightList<UISlotContentTemplate> slotContents;
        
        public TemplateScope(UIElement rootElement) {
            this.rootElement = rootElement;
            this.slotContents = new LightList<UISlotContentTemplate>();
        }

        public UISlotContentTemplate FindSlotContent(string slotName) {
            for (int i = 0; i < slotContents.Count; i++) {
                if (slotContents[i].SlotName == slotName) {
                    return slotContents[i];
                }
            }
            return null;
        }

    }

}