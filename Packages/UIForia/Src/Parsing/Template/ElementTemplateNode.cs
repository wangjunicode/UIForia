using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class ElementTemplateNode : TemplateNode {

        public string templateName;
        public LightList<SlotNode> slotDefinitionNodes;
        public TemplateShell templateShell;

        public ElementTemplateNode(string templateName, TemplateShell templateShell, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) : base(null, null, processedType, attributes, in templateLineInfo) {
            this.templateName = templateName;
            this.templateShell = templateShell;
        }

        public void AddSlot(SlotNode slotNode) {
            slotDefinitionNodes = slotDefinitionNodes ?? new LightList<SlotNode>(4);
            for (int i = 0; i < slotDefinitionNodes.size; i++) {
                if (slotDefinitionNodes.array[i].slotName == slotNode.slotName) {
                    //   throw ParseException.MultipleSlotsWithSameName(filePath, slotDefinitionNode.slotName);
                }
            }

            slotDefinitionNodes.Add(slotNode);
        }

        public bool HasSlotExternOverride(string slotName, out SlotNode slotNode) {
            if (slotDefinitionNodes == null || slotDefinitionNodes.size == 0) {
                slotNode = null;
                return false;
            }

            for (int i = 0; i < slotDefinitionNodes.size; i++) {
                if (slotDefinitionNodes.array[i].slotName == slotName) {
                    slotNode = slotDefinitionNodes.array[i];
                    return true;
                }
            }

            slotNode = null;
            return false;
        }

        public bool DefinesSlot(string slotName) {
            if (slotDefinitionNodes == null || slotDefinitionNodes.size == 0) {
                // slotNode = null;
                return false;
            }

            for (int i = 0; i < slotDefinitionNodes.size; i++) {
                if (slotDefinitionNodes.array[i].slotName == slotName) {
                    //  slotNode = slotDefinitionNodes.array[i];
                    return true;
                }
            }

            //  slotNode = null;
            return false;
        }

    }

}