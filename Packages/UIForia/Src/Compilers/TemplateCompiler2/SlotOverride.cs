
using UIForia.Elements;
using UIForia.Parsing;

namespace UIForia.Compilers {

    public struct SlotOverride {

        public readonly string slotName;
        public readonly int templateId;
        public readonly TemplateData templateData;
        public readonly SlotType slotType;
        public readonly UIElement root;

        public SlotOverride(string slotName, UIElement root, TemplateData templateData, int templateId, SlotType slotType) {
            this.slotName = slotName;
            this.root = root;
            this.templateData = templateData;
            this.templateId = templateId;
            this.slotType = slotType;
        }

    }

}