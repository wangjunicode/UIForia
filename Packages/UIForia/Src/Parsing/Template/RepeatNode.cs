using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Parsing {

    public class RepeatNode : TemplateNode {

        public RepeatNode(ReadOnlySizedArray<AttributeDefinition> attributes, in LineInfo lineInfo) : base(attributes, in lineInfo) {
            // todo -- 1. cache, 2. what about repeat count? treat as slot-type template?
            processedType = TypeProcessor.GetProcessedType(typeof(UIRepeatElement<>));
        }

        public string GetItemVariableName() {
        
            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];
                if (attr.type == AttributeType.ImplicitVariable) {
                    if (attr.value == "item") {
                        return attr.key;
                    }
                }
            }

            return "item";
        }

        public string GetIndexVariableName() {

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];
                if (attr.type == AttributeType.ImplicitVariable) {
                    if (attr.value == "index") {
                        return attr.key;
                    }
                }
            }

            return "index";
        }

        public override string GetTagName() {
            return "Repeat";
        }
        
    }

}