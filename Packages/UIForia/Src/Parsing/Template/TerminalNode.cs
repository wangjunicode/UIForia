using UIForia.Exceptions;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class TerminalNode : TemplateNode2 {

        public TerminalNode(RootTemplateNode root, TemplateNode2 parent, ProcessedType processedType, StructList<AttributeDefinition2> attributes, in TemplateLineInfo templateLineInfo) : base(root, parent, processedType, attributes, templateLineInfo) { }

        public override void AddChild(TemplateNode2 child) {
            throw new InvalidArgumentException($"{root.filePath} -> Terminal element {processedType.rawType} cannot accept children. {lineInfo.line}:{lineInfo.column}");
        }

    }

}