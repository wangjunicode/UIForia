using System;
using System.Linq.Expressions;
using Mono.Linq.Expressions;
using UIForia.Parsing;
using UIForia.Systems;
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

        private static readonly string s_ElementFnTypeName = typeof(Action<ElementSystem>[]).GetTypeName();
        private static readonly string s_BindingFnTypeName = typeof(Action<LinqBindingNode>[]).GetTypeName();

        public IndentedStringBuilder ToCSharpCode(IndentedStringBuilder stringBuilder) {

            stringBuilder.AppendInline("new ");
            stringBuilder.AppendInline(nameof(TemplateData));
            stringBuilder.AppendInline(" (");
            stringBuilder.AppendInline("\"");
            stringBuilder.AppendInline(processedType.tagName);
            stringBuilder.AppendInline("\"");
            stringBuilder.AppendInline(") {");
            stringBuilder.NewLine();
            stringBuilder.Indent();
            stringBuilder.Indent();

            BuildEntryPoint(stringBuilder);
            stringBuilder.NewLine();

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
                stringBuilder.Append(elementTemplates[i].expression.ToTemplateBody(3));
                if (i != elementTemplates.Length - 1) {
                    stringBuilder.AppendInline(",\n");
                }
            }

            stringBuilder.Outdent();

            stringBuilder.NewLine();
            stringBuilder.Append("},");
            
            stringBuilder.NewLine();
            
            stringBuilder.Append(nameof(TemplateData.bindings));
            stringBuilder.AppendInline(" = new ");
            stringBuilder.AppendInline(s_BindingFnTypeName);
            stringBuilder.AppendInline(" {\n");
            stringBuilder.Indent();

            for (int i = 0; i < bindings.Length; i++) {
                stringBuilder.Append("// ");
                stringBuilder.AppendInline(i.ToString());
                stringBuilder.NewLine();
                stringBuilder.Append(bindings[i].ToTemplateBody(3));
                if (i != bindings.Length - 1) {
                    stringBuilder.AppendInline(",\n");
                }
            }
            stringBuilder.NewLine();
            stringBuilder.Outdent();
            stringBuilder.Append("}");
            
            stringBuilder.NewLine();
            stringBuilder.Outdent();
            stringBuilder.Append("};");
            return stringBuilder;
        }

        private void BuildEntryPoint(IndentedStringBuilder stringBuilder) {
            stringBuilder.Indent();
            stringBuilder.Append(nameof(TemplateData.entry));
            stringBuilder.AppendInline(" = ");
            stringBuilder.AppendInline(entryPoint.ToTemplateBody(2));
            stringBuilder.AppendInline(",");
            stringBuilder.Outdent();
        }

        private void BuildHydratePoint(IndentedStringBuilder stringBuilder) {
            stringBuilder.Indent();
            stringBuilder.Append(nameof(TemplateData.hydrate));
            stringBuilder.AppendInline(" = ");
            stringBuilder.AppendInline(hydratePoint.ToTemplateBody(2));
            stringBuilder.AppendInline(",");
            stringBuilder.Outdent();
        }

    }

}