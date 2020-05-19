using UIForia.Exceptions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class ExpandedNode : ElementNode {

        internal SizedArray<SlotNode> slotOverrideNodes;

        public ExpandedNode(string moduleName, string tagName, ReadOnlySizedArray<AttributeDefinition> attributes, in LineInfo lineInfo)
            : base(moduleName, tagName, attributes, in lineInfo) { }

        internal SlotNode FindOrCreateChildrenSlotOverride() {
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

        public bool TryGetSlotOverride(string slotName, out SlotNode slotNode) {
            for (int i = 0; i < slotOverrideNodes.size; i++) {
                if (slotOverrideNodes.array[i].slotName == slotName) {
                    slotNode = slotOverrideNodes.array[i];
                    return true;
                }
            }

            slotNode = null;
            return false;
        }

    }

}