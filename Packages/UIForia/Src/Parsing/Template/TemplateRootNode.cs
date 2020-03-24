using UIForia.Util;

namespace UIForia.Parsing {

    public class TemplateRootNode : TemplateNode {

        public readonly string templateName;
        public readonly TemplateShell templateShell;

        internal LightList<SlotNode> slotDefinitionNodes;

        public TemplateRootNode(string templateName, TemplateShell templateShell, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo)
            : base(attributes, in templateLineInfo) {
            this.templateName = templateName;
            this.templateShell = templateShell;
        }

        internal void AddSlot(SlotNode slotNode) {
            slotDefinitionNodes = slotDefinitionNodes ?? new LightList<SlotNode>(4);
            slotDefinitionNodes.Add(slotNode);
        }

        public override string GetTagName() {
            return "Contents";
        }

    }

}