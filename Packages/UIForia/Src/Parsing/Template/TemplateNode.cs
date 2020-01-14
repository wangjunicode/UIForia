using System;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public abstract class TemplateNode {

        public StructList<AttributeDefinition2> attributes;
        public LightList<TemplateNode> children;
        public ElementTemplateNode elementRoot;
        public TemplateNode parent;
        public ProcessedType processedType;
        public string originalString;
        public string tagName;
        public string namespaceName;
        public TemplateLineInfo lineInfo;

        protected TemplateNode(ElementTemplateNode elementRoot, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) {
            this.elementRoot = elementRoot;
            this.parent = parent;
            this.attributes = attributes;
            this.processedType = processedType;
            this.lineInfo = templateLineInfo;
        }

        public virtual void AddChild(TemplateNode child) {
            children = children ?? new LightList<TemplateNode>();
            children.Add(child);
        }


        public bool HasAttribute(string attr) {
            if (attributes == null) return false;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Attribute) {
                    if (attributes.array[i].key == attr) {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public bool HasProperty(string attr) {
            if (attributes == null) return false;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Property) {
                    if (attributes.array[i].key == attr) {
                        return true;
                    }
                }
            }

            return false;
        }

        public TemplateNode this[int i] => children?.array[i];

        public int ChildCount => children?.size ?? 0;
        public Type ElementType => processedType.rawType;

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