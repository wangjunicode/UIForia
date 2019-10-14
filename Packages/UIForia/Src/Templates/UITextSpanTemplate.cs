using System;
using System.Collections.Generic;
using UIForia.Bindings;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Extensions;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Templates {

    public class UITextSpanTemplate : UITemplate {
        
        public string RawText { get; }

        public UITextSpanTemplate(Application app, string rawText, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) : base(app, childTemplates, attributes) {
            this.RawText = rawText;
        }

        protected override Type elementType => typeof(TextSpanElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            TextSpanElement element = new TextSpanElement();
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            element.children = LightList<UIElement>.Get();
            element.OriginTemplate = this;
            return element;
        }

        public override void Compile(ParsedTemplate template) {
            StructList<TextExpression> list = StructList<TextExpression>.Get();
            
            TextTemplateProcessor.ProcessTextExpressions(RawText, list);

            LightList<Expression<string>> expressionList = LightList<Expression<string>>.Get();
            
            expressionList.EnsureCapacity(list.size);
            
            for (int i = 0; i < list.size; i++) {
                Expression<string> expression;
                if (list[i].isExpression) {
                    expression = template.compiler.Compile<string>(template.RootType, elementType, list[i].text);
                }
                else {
                    expression = new LiteralExpression_String(list[i].text);
                }

                expressionList.Add(expression);
            }

            StructList<TextExpression>.Release(ref list);
            
            if (expressionList.Count == 1) {
                Binding binding = new TextBinding_Single(expressionList[0]);
                if (binding.IsConstant()) {
                    binding.bindingType = BindingType.Constant;
                }
                s_BindingList.Add(binding);
            }
            else if (expressionList.Count > 1) {
                Binding binding = new TextBinding_Multiple(expressionList.ToArray());
                if (binding.IsConstant()) {
                    binding.bindingType = BindingType.Constant;
                }
                s_BindingList.Add(binding);
            }

            LightList<Expression<string>>.Release(ref expressionList);

            base.Compile(template);
        }

    }

}