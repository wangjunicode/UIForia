using System;
using System.Collections.Generic;
using UIForia.Bindings;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expression;
using UIForia.Util;

namespace UIForia.Templates {

    public class UITextSpanTemplate : UITemplate {

        private static readonly TextElementParser textParser = new TextElementParser();

        public string RawText { get; }

        public UITextSpanTemplate(Application app, string rawText, List<UITemplate> childTemplates, List<AttributeDefinition> attributes = null) : base(app, childTemplates, attributes) {
            this.RawText = rawText;
        }

        protected override Type elementType => typeof(TextSpanElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            TextSpanElement element = new TextSpanElement(RawText);
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            element.children = LightList<UIElement>.Get();
            element.OriginTemplate = this;
            return element;
        }

        public override void Compile(ParsedTemplate template) {
            // todo -- if part is constant and value is empty string, remove it
            string[] expressionParts = textParser.Parse(RawText);

            List<Expression<string>> expressionList = ListPool<Expression<string>>.Get();

            for (int i = 0; i < expressionParts.Length; i++) {
                if (expressionParts[i] == "''") {
                    continue;
                }

                Expression<string> expression = template.compiler.Compile<string>(template.RootType, elementType, expressionParts[i]);
                expressionList.Add(expression);
            }

            // todo -- if constant, try to merge bindings. This probably happens in the binding system
            // todo    because we need to have access to the element and an expression context

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

            ListPool<Expression<string>>.Release(ref expressionList);

            base.Compile(template);
        }

    }

}