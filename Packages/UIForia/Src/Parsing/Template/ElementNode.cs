using System;
using UIForia.Exceptions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class ElementNode : TemplateNode {

        public readonly string moduleName;
        public readonly string tagName;
        public LightList<SlotNode> slotOverrideNodes;

        public ElementNode(string moduleName, string tagName, ReadOnlySizedArray<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo)
            : base(attributes, templateLineInfo) {
            this.moduleName = moduleName;
            this.tagName = tagName;
        }

        // public override void AddChild(TemplateNode child) {
        //     if (child is SlotNode slotNode) {
        //         AddSlotOverride(slotNode);
        //     }
        //     else {
        //         SlotNode childrenSlot = FindOrCreateChildrenSlotOverride();
        //         childrenSlot.AddChild(child);
        //     }
        // }

        public override string GetTagName() {
            if (!string.IsNullOrEmpty(moduleName)) {
                return moduleName + ":" + tagName;
            }

            return tagName;
        }

        internal LightList<TemplateNode> OrphanChildren() {
            LightList<TemplateNode> retn = children;
            children = null;
            return retn;
        }

        internal SlotNode FindOrCreateChildrenSlotOverride() {
            if (slotOverrideNodes == null) {
                slotOverrideNodes = new LightList<SlotNode>(1);
            }

            for (int i = 0; i < slotOverrideNodes.size; i++) {
                if (slotOverrideNodes.array[i].slotName == "Children") {
                    return slotOverrideNodes.array[i];
                }
            }

            SlotNode slot = new SlotNode("Children", default, default, lineInfo, SlotType.Override) {
                root = root,
                parent = this
            };

            slotOverrideNodes.Add(slot);

            return slot;
        }

        public override void AddSlotOverride(SlotNode node) {
            slotOverrideNodes = slotOverrideNodes ?? new LightList<SlotNode>(2);
            for (int i = 0; i < slotOverrideNodes.size; i++) {
                if (slotOverrideNodes.array[i].slotName == node.slotName) {
                    throw MultipleSlotOverrides(node.slotName);
                }
            }

            slotOverrideNodes.Add(node);
        }

        private ParseException MultipleSlotOverrides(string nodeSlotName) {
            return new ParseException($"Slot with name {nodeSlotName} was overridden multiple times, which is invalid");
        }

    }

}