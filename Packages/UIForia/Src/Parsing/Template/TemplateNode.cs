using System;
using System.Xml.Linq;
using UIForia.Parsing.Expressions;
using UIForia.Text;
using UIForia.Util;

namespace UIForia.Parsing {

    public struct TemplateNodeDebugData {

        public string fileName;
        public string tagName;
        public TemplateLineInfo lineInfo;

        public override string ToString() {
            return $"<{tagName}> @({fileName} line {lineInfo})";
        }

    }

    public struct AttributeNodeDebugData {

        public string fileName;
        public string tagName;
        public string content;
        public TemplateLineInfo lineInfo;

        public AttributeNodeDebugData(string fileName, string tagName, TemplateLineInfo lineInfo, string content) {
            this.fileName = fileName;
            this.tagName = tagName;
            this.lineInfo = lineInfo;
            this.content = content;
        }
    }
    
        
    public abstract class TemplateNode {

        // todo -- try make these lists into arrays
        public LightList<TemplateNode> children;
        public StructList<AttributeDefinition> attributes;
        public TemplateRootNode root;
        public TemplateNode parent;
        public ProcessedType processedType;
        public string originalString;
        public TemplateLineInfo lineInfo;
        public string genericTypeResolver;
        public string requireType;
        public bool isModified;

        protected TemplateNode(StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo) {
            this.attributes = attributes;
            this.lineInfo = templateLineInfo;
        }

        public virtual void AddChild(TemplateNode child) {
            child.parent = this;
            children = children ?? new LightList<TemplateNode>();
            children.Add(child);
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
        
        public virtual TemplateNodeDebugData TemplateNodeDebugData => new TemplateNodeDebugData() {
            lineInfo = lineInfo,
            tagName = "",
            fileName = root != null ? root.templateShell.filePath : ((TemplateRootNode)this).templateShell.filePath
        };

        public abstract string GetTagName();

        public virtual void AddSlotOverride(SlotNode slotNode) {
            throw new NotSupportedException($"Cannot add a <{slotNode.GetTagName()}> to <{GetTagName()}>");
        }

    }

}