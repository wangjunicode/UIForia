using UIForia.Parsing.Expressions;

namespace UIForia {

    public class TemplateEditorAttributeNode {

        public string name;
        public string value;
        public AttributeType type;
        internal AttributeFlags flags;
        
        public TemplateEditorAttributeNode(AttributeDefinition2 attr) {
            name = attr.key;
            value = attr.value;
            type = attr.type;
            flags = attr.flags;
        }

        public bool isConst {
            get => (flags & AttributeFlags.Const) != 0;
            set {
                if (value) {
                    flags |= AttributeFlags.Const;
                }
                else {
                    flags &= ~AttributeFlags.Const;
                }
            }
        }
    }

}