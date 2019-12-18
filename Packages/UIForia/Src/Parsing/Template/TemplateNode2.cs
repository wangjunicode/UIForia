using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public abstract class TemplateNode2 {

        public StructList<AttributeDefinition2> attributes;
        private LightList<TemplateNode2> children;
        public RootTemplateNode root;
        public TemplateNode2 parent;
        public ProcessedType processedType;
        public string originalString;
        public string tagName;
        public string namespaceName;
        public TemplateLineInfo lineInfo;

        protected TemplateNode2(RootTemplateNode root, TemplateNode2 parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) {
            this.root = root;
            this.parent = parent;
            this.attributes = attributes;
            this.processedType = processedType;
            this.lineInfo = templateLineInfo;
        }

        public virtual void AddChild(TemplateNode2 child) {
            children = children ?? new LightList<TemplateNode2>(8);
            children.Add(child);
        }

        public TemplateNode2 this[int i] => children?.array[i];

        public int ChildCount => children?.size ?? 0;

        public int GetAttributeCount() {
            if (attributes == null) {
                return 0;
            }

            int attrCnt = 0;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Attribute) {
                    attrCnt++;
                }
            }

            return attrCnt;
        }

    }

}