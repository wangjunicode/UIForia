using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class ChildrenNode : SlotNode {

        public ChildrenNode(TemplateRootNode root, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) 
            : base(root, parent, processedType, attributes, templateLineInfo, "Children", SlotType.Children) { }

    }

}