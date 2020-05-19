using UIForia.Util;

namespace UIForia.Parsing {

    public class ContainerNode : ElementNode {

        public ContainerNode(string moduleName, string tagName, ReadOnlySizedArray<AttributeDefinition> attributes, in LineInfo lineInfo)
            : base(moduleName, tagName, attributes, in lineInfo) { }

    }

}