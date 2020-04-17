using System.Text;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class TextNode : TemplateNode {

        public readonly StructList<TextExpression> textExpressionList;

        public TextNode(ProcessedType processedType, ReadOnlySizedArray<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo)
            : base(attributes, templateLineInfo) {
            this.textExpressionList = new StructList<TextExpression>(3);
            this.attributes = attributes;
            this.processedType = processedType;
        }

        public bool IsTextConstant() {
            if (textExpressionList == null || textExpressionList.size == 0) return false;

            for (int i = 0; i < textExpressionList.Count; i++) {
                if (textExpressionList.array[i].isExpression) {
                    return false;
                }
            }

            return true;
        }

        public string GetStringContent() {
            if (textExpressionList == null) return string.Empty;
            StringBuilder builder = StringUtil.GetPerThreadStringBuilder();
            
            builder.Clear();

            for (int i = 0; i < textExpressionList.Count; i++) {
                builder.Append(textExpressionList[i].text);
            }

            string retn = builder.ToString();
            StringUtil.ReleasePerThreadStringBuilder(builder);
            return retn;
        }

        public override string GetTagName() {
            return "Text";
        }

    }

}