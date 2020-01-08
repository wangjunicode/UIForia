using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public enum SlotType {

        Default,
        Children,
        Extern,
        Template,
        Override

    }
    
    public class SlotNode : TemplateNode2 {

        public string slotName;
        public SlotType slotType;
        public int compiledSlotId;

        public  SlotNode(ElementTemplateNode root, TemplateNode2 parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo, string slotName, SlotType slotType)
            : base(root, parent, processedType, attributes, templateLineInfo) {
            this.compiledSlotId = -1;
            this.slotName = slotName;
            this.slotType = slotType;
        }

    }

}