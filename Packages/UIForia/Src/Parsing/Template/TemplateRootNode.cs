using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class TemplateRootNode : TemplateNode {

        public readonly string templateName;
        public readonly TemplateShell templateShell;
        
        public LightList<SlotNode> slotDefinitionNodes;

        public TemplateRootNode(string templateName, TemplateShell templateShell, ProcessedType processedType, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo) : base(null, null, processedType, attributes, in templateLineInfo) {
            this.templateName = templateName;
            this.templateShell = templateShell;
        }

        public TemplateRootNode(TemplateRootNode other) : base(other) {
            templateName = other.templateName;
            templateShell = other.templateShell;
            slotDefinitionNodes = other.slotDefinitionNodes;
        }

        public override object Clone() {
            return new TemplateRootNode(this);
        }

        public void AddSlot(SlotNode slotNode) {
            slotDefinitionNodes = slotDefinitionNodes ?? new LightList<SlotNode>(4);
            slotDefinitionNodes.Add(slotNode);
        }

        public bool DefinesSlot(string slotName, out SlotNode slotNode) {
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

        // Recursively resets resolved generic types of children.
        // This forces type resolution for nested generic types.
        private static LightList<TemplateNode> ResetGenericTypes(LightList<TemplateNode> children) {
            LightList<TemplateNode> newChildren = new LightList<TemplateNode>(children);
            
            for (int i = 0; i < newChildren.size; ++i) {
                TemplateNode child = newChildren[i];
                if (child.processedType.rawType.IsGenericType && !(child is RepeatNode)) {
                    TemplateNode clone = (TemplateNode)child.Clone();
                    clone.processedType = TypeProcessor.GetProcessedType(clone.processedType.rawType.GetGenericTypeDefinition());
                    newChildren[i] = clone;
                }

                if (child.children != null && child.children.size > 0) {
                    child.children = ResetGenericTypes(child.children);
                }
            }

            return newChildren;
        }

        public TemplateRootNode Clone(ProcessedType overrideType) {
            TemplateRootNode rootNode = new TemplateRootNode(templateName, templateShell, overrideType, attributes, lineInfo);
            
            rootNode.children = ResetGenericTypes(children);
            rootNode.slotDefinitionNodes = slotDefinitionNodes;
            
            return rootNode;
        }

    }

}