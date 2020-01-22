using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class ExpandedTemplateNode : TemplateNode {

        public TemplateRootNode expandedRoot;
        public LightList<SlotNode> slotOverrideNodes;

        public ExpandedTemplateNode(TemplateRootNode root, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) 
            : base(root, parent, processedType, attributes, templateLineInfo) { }

        public override void AddChild(TemplateNode child) {
            SlotNode childrenSlot = FindOrCreateSlotOverride("Children");
            childrenSlot.AddChild(child);
        }

        private SlotNode FindOrCreateSlotOverride(string slotName) {
            if (slotOverrideNodes == null) {
                slotOverrideNodes = new LightList<SlotNode>(2);
                SlotNode slot = new SlotNode(root, null, TypeProcessor.GetProcessedType(typeof(UISlotOverride)), attributes, lineInfo, slotName, SlotType.Override);

                slotOverrideNodes.Add(slot);
                return slot;
            }
            else {
                for (int i = 0; i < slotOverrideNodes.size; i++) {
                    if (slotOverrideNodes.array[i].slotName == slotName) {
                        return slotOverrideNodes.array[i];
                    }
                }

                SlotNode slot = new SlotNode(root, null, TypeProcessor.GetProcessedType(typeof(UISlotOverride)), attributes, lineInfo, slotName, SlotType.Override);

                slotOverrideNodes.Add(slot);
                return slot;
            }
        }

        public void AddSlotOverride(SlotNode node) {
            slotOverrideNodes = slotOverrideNodes ?? new LightList<SlotNode>(2);
            for (int i = 0; i < slotOverrideNodes.size; i++) {
                if (slotOverrideNodes.array[i].slotName == node.slotName) {
                    throw ParseException.MultipleSlotOverrides(node.slotName);
                }
            }

            slotOverrideNodes.Add(node);
        }

        public void ValidateSlot(string slotName, in TemplateLineInfo slotLineInfo) {
            if (expandedRoot.slotDefinitionNodes == null || expandedRoot.slotDefinitionNodes.size == 0) {
                throw ParseException.SlotNotFound(root.templateShell.filePath, slotName, slotLineInfo);
            }

            LightList<SlotNode> slotDefinitions = expandedRoot.slotDefinitionNodes;
            for (int i = 0; i < slotDefinitions.size; i++) {
                if (slotDefinitions.array[i].slotName == slotName) {
                    return;
                }
            }

            throw ParseException.SlotNotFound(root.templateShell.filePath, slotName, slotLineInfo);
        }

    }

}