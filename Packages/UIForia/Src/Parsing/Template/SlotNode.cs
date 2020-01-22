using UIForia.Compilers;
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

    public class SlotNode : TemplateNode {

        public string slotName;
        public SlotType slotType;

        public CompiledSlot compiledSlot;

        public SlotNode(TemplateRootNode root, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo, string slotName, SlotType slotType)
            : base(root, parent, processedType, attributes, templateLineInfo) {
            this.slotName = slotName;
            this.slotType = slotType;
        }
        
        public AttributeDefinition2[] GetAttributes(AttributeType expose) {
            if (attributes == null) {
                return null;
            }

            int cnt = 0;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == expose) {
                    cnt++;
                }
            }

            if (cnt == 0) return null;
            int idx = 0;
            AttributeDefinition2[] retn = new AttributeDefinition2[cnt];
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == expose) {
                    retn[idx++] = attributes.array[i];
                }
            }

            return retn;
        }

    }

}