using UIForia.Parsing.Expressions;

namespace UIForia {

    public class TemplateEditorAttributeNode {

        public readonly string key;
        public readonly string value;
        public readonly AttributeType type;
        
        internal AttributeFlags flags;
        
        public TemplateEditorAttributeNode(string attrKey, string attrValue, AttributeType attrType, AttributeFlags attrFlags) {
            this.key = attrKey;
            this.value = attrValue;
            this.type = attrType;
            this.flags = attrFlags;
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