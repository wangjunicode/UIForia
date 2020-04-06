using System;
using System.Linq.Expressions;
using Mono.Linq.Expressions;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public struct TemplateOutput {

        public TemplateNode templateNode;
        public LambdaExpression expression;

    }

    public class TemplateExpressionSet {

        public ProcessedType processedType;
        public LambdaExpression entryPoint;
        public LambdaExpression hydratePoint;
        public LambdaExpression[] bindings;
        public TemplateOutput[] elementTemplates;
        public LambdaExpression[] slotTemplates;

        private static readonly string s_TypeName = typeof(TemplateData).GetTypeName();
        private static readonly string s_ElementFnTypeName = typeof(Action<ElementSystem>[]).GetTypeName();

        public IndentedStringBuilder ToCSharpCode(IndentedStringBuilder stringBuilder) {
            if (!string.IsNullOrEmpty(processedType.templateId)) {
                stringBuilder.AppendInline(" templateId = ");
                stringBuilder.AppendInline(processedType.templateId);
            }

            stringBuilder.NewLine();
            stringBuilder.Append("new ");
            stringBuilder.AppendInline(s_TypeName);
            stringBuilder.AppendInline(" (");
            stringBuilder.AppendInline("\"");
            stringBuilder.AppendInline(processedType.tagName);
            stringBuilder.AppendInline("\"");
            stringBuilder.AppendInline(") {\n");
            BuildEntryPoint(stringBuilder);
            stringBuilder.AppendInline("\n");
            BuildHydratePoint(stringBuilder);

            stringBuilder.NewLine();
            stringBuilder.Indent();
            stringBuilder.Append(nameof(TemplateData.elements));
            stringBuilder.AppendInline(" = new ");
            stringBuilder.AppendInline(s_ElementFnTypeName);
            stringBuilder.AppendInline(" {\n");

            stringBuilder.Indent();
            for (int i = 0; i < elementTemplates.Length; i++) {
                stringBuilder.Append("//");
                stringBuilder.AppendInline(i.ToString());
                stringBuilder.AppendInline(" <");
                stringBuilder.AppendInline(elementTemplates[i].templateNode.GetTagName());
                stringBuilder.AppendInline("> line ");
                stringBuilder.AppendInline(elementTemplates[i].templateNode.lineInfo.ToString());
                stringBuilder.NewLine();
                stringBuilder.Append(elementTemplates[i].expression.ToTemplateBody(4));
                if (i != elementTemplates.Length - 1) {
                    stringBuilder.AppendInline(",\n");
                }
            }

            stringBuilder.NewLine();
            stringBuilder.Outdent();
            stringBuilder.Append("}");
            stringBuilder.NewLine();
            stringBuilder.Outdent();
            stringBuilder.Append("};\n");
            return stringBuilder;
        }

        private void BuildEntryPoint(IndentedStringBuilder stringBuilder) {
            stringBuilder.Indent();
            stringBuilder.Append(nameof(TemplateData.entry));
            stringBuilder.AppendInline(" = ");
            stringBuilder.AppendInline(entryPoint.ToTemplateBody(3));
            stringBuilder.AppendInline(",");
            stringBuilder.Outdent();
        }

        private void BuildHydratePoint(IndentedStringBuilder stringBuilder) {
            stringBuilder.Indent();
            stringBuilder.Append(nameof(TemplateData.hydrate));
            stringBuilder.AppendInline(" = ");
            stringBuilder.AppendInline(hydratePoint.ToTemplateBody(3));
            stringBuilder.AppendInline(",");
            stringBuilder.Outdent();
        }

    }

}