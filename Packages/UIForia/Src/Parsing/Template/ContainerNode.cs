using UIForia.Util;

namespace UIForia.Parsing {

    public class ContainerNode : ElementNode {

        public ContainerNode(string moduleName, string tagName, ReadOnlySizedArray<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo)
            : base(moduleName, tagName, attributes, in templateLineInfo) { }

    }

}