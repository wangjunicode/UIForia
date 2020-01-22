using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class RepeatNode : TemplateNode {

        public RepeatNode(TemplateRootNode root, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) : base(root, parent, processedType, attributes, in templateLineInfo) { }

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

}