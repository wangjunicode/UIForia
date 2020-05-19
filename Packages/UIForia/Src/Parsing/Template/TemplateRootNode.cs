using UIForia.Util;

namespace UIForia.Parsing {

    public class TemplateRootNode : TemplateNode {

        public readonly string templateName;
        public readonly TemplateShell_Deprecated templateShell;

        private SizedArray<SlotNode> slotDefinitionNodes;

        public TemplateRootNode(string templateName, TemplateShell_Deprecated templateShell, ReadOnlySizedArray<AttributeDefinition> attributes, in LineInfo lineInfo)
            : base(attributes, in lineInfo) {
            this.templateName = templateName;
            this.templateShell = templateShell;
            this.root = this;
            this.slotDefinitionNodes = default;
        }

        internal void AddSlot(SlotNode slotNode) {
            slotDefinitionNodes.Add(slotNode);
        }

        public override string GetTagName() {
            return "Contents";
        }

        public string DebugDump() {

            
            IndentedStringBuilder stringBuilder = new IndentedStringBuilder(512);
        
            stringBuilder.Append(GetTagName());
            
            stringBuilder.Indent();
            
            for (int i = 0; i < children.size; i++) {
                children.array[i].DebugDump(stringBuilder);
            }
            
            stringBuilder.Outdent();

            return stringBuilder.ToString();

        }

        public bool TryGetSlotNode(string slotName, out SlotNode slotNode) {
            for (int i = 0; i < slotDefinitionNodes.size; i++) {
                if (slotName == slotDefinitionNodes.array[i].slotName) {
                    slotNode = slotDefinitionNodes.array[i];
                    return true;
                }
            }

            slotNode = null;
            return false;
        }

    }

}