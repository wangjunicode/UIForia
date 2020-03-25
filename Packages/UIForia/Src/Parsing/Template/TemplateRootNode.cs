using UIForia.Util;

namespace UIForia.Parsing {

    public class TemplateRootNode : TemplateNode {

        public readonly string templateName;
        public readonly TemplateShell templateShell;

        internal LightList<SlotNode> slotDefinitionNodes;

        public TemplateRootNode(string templateName, TemplateShell templateShell, ReadOnlySizedArray<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo)
            : base(attributes, in templateLineInfo) {
            this.templateName = templateName;
            this.templateShell = templateShell;
            this.root = this;
        }

        internal void AddSlot(SlotNode slotNode) {
            slotDefinitionNodes = slotDefinitionNodes ?? new LightList<SlotNode>(4);
            slotDefinitionNodes.Add(slotNode);
        }

        public override string GetTagName() {
            return "Contents";
        }

        public string DebugDump() {

            if (children == null) {
                return $"{GetTagName()}";
            }

            IndentedStringBuilder stringBuilder = new IndentedStringBuilder(512);
        
            stringBuilder.Append(GetTagName());
            
            stringBuilder.Indent();
            
            for (int i = 0; i < children.size; i++) {
                children.array[i].DebugDump(stringBuilder);
            }
            
            stringBuilder.Outdent();

            return stringBuilder.ToString();

        }

    }

}