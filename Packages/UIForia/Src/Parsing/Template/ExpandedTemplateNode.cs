using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class RepeatNode : TemplateNode {

        public RepeatNode(ElementTemplateNode elementRoot, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) : base(elementRoot, parent, processedType, attributes, in templateLineInfo) { }

        public string GetItemVariableName() {
            if (attributes == null) {
                return "item";
            }

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition2 attr = ref attributes.array[i];
                if (attr.type == AttributeType.ImplicitVariable) {
                    if (attr.key == "item") {
                        return attr.value.Trim();
                    }
                }
            }

            return "item";
        }

        public string GetIndexVariableName() {
            if (attributes == null) {
                return "index";
            }

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition2 attr = ref attributes.array[i];
                if (attr.type == AttributeType.ImplicitVariable) {
                    if (attr.key == "index") {
                        return attr.value.Trim();
                    }
                }
            }

            return "index";
        }

    }

    public class ExpandedTemplateNode : TemplateNode {

        public readonly ElementTemplateNode expandedRoot;
        public LightList<SlotNode> slotOverrideNodes;

        public ExpandedTemplateNode(ElementTemplateNode expandedRoot, ElementTemplateNode elementRoot, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) : base(expandedRoot, parent, processedType, attributes, templateLineInfo) {
            this.expandedRoot = expandedRoot;
        }

        public override void AddChild(TemplateNode child) {
            SlotNode childrenSlot = FindOrCreateSlotOverride("Children");
            childrenSlot.AddChild(child);
        }

        private SlotNode FindOrCreateSlotOverride(string slotName) {
            if (slotOverrideNodes == null) {
                slotOverrideNodes = new LightList<SlotNode>(4);
                SlotNode slot = new SlotNode(elementRoot, null, TypeProcessor.GetProcessedType(typeof(UISlotOverride)), attributes, lineInfo, slotName, SlotType.Override);

                slotOverrideNodes.Add(slot);
                return slot;
            }
            else {
                for (int i = 0; i < slotOverrideNodes.size; i++) {
                    if (slotOverrideNodes.array[i].slotName == slotName) {
                        return slotOverrideNodes.array[i];
                    }
                }

                SlotNode slot = new SlotNode(elementRoot, null, TypeProcessor.GetProcessedType(typeof(UISlotOverride)), attributes, lineInfo, slotName, SlotType.Override);

                slotOverrideNodes.Add(slot);
                return slot;
            }
        }

        public void AddSlotOverride(SlotNode node) {
            slotOverrideNodes = slotOverrideNodes ?? new LightList<SlotNode>(4);
            for (int i = 0; i < slotOverrideNodes.size; i++) {
                if (slotOverrideNodes.array[i].slotName == node.slotName) {
                    throw ParseException.MultipleSlotOverrides(node.slotName);
                }
            }

            slotOverrideNodes.Add(node);
        }

        public void ValidateSlot(string slotName, in TemplateLineInfo lineInfo) {
            if (expandedRoot.slotDefinitionNodes == null || expandedRoot.slotDefinitionNodes.size == 0) {
                throw ParseException.SlotNotFound(elementRoot.templateShell.filePath, slotName, lineInfo);
            }
            else {
                LightList<SlotNode> slotDefinitions = expandedRoot.slotDefinitionNodes;
                for (int i = 0; i < slotDefinitions.size; i++) {
                    if (slotDefinitions.array[i].slotName == slotName) {
                        return;
                    }
                }
            }

            throw ParseException.SlotNotFound(elementRoot.templateShell.filePath, slotName, lineInfo);
        }

    }

}