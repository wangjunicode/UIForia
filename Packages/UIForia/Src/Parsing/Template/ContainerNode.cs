using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class ContainerNode : TemplateNode2 {

        public ContainerNode(ElementTemplateNode root, TemplateNode2 parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) : base(root, parent, processedType, attributes, templateLineInfo) { }

    }

}