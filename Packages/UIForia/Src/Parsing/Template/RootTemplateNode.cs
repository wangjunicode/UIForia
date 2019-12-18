using UIForia.Exceptions;
using UIForia.Parsing.Expressions;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Parsing {

    public class RootTemplateNode : TemplateNode2 {

        public readonly string filePath;
        public StructList<UsingDeclaration> usings;
        public StructList<StyleDefinition> styles;
        public readonly string fullFilePath;

        public RootTemplateNode(string fullFilePath, string filePath, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) : base(null, null, processedType, attributes, templateLineInfo) {
            this.fullFilePath = fullFilePath;
            this.filePath = filePath;
        }

        public LightList<SlotNode> slotDefinitionNodes;

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

    }

}