using System;
using System.Collections.Generic;
using UIForia.Bindings;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expression;
using UIForia.Util;

namespace UIForia.Templates {

    public class UITextTemplate : UITemplate {

        private static readonly TextElementParser textParser = new TextElementParser();
        private readonly Type textElementType;

        public UITextTemplate(Application app, Type textElementType, string rawText, List<AttributeDefinition> attributes = null)
            : base(app, null, attributes) {
            this.textElementType = textElementType;
            this.RawText = rawText;
            this.elementName = "Text";
        }

        public UITextTemplate(Application app, string rawText, List<AttributeDefinition> attributes = null)
            : base(app, null, attributes) {
            this.textElementType = null;
            this.RawText = rawText;
            this.elementName = "Text";
        }

        public string RawText { get; }

        public override void Compile(ParsedTemplate template) {
            // todo -- if part is constant and value is empty string, remove it
            string[] expressionParts = textParser.Parse(RawText);

            List<Expression<string>> expressionList = ListPool<Expression<string>>.Get();

            for (int i = 0; i < expressionParts.Length; i++) {
                if (expressionParts[i] == "''") {
                    continue;
                }

                Expression<string> expression = template.compiler.Compile<string>(template.RootType, textElementType, expressionParts[i]);
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

            // todo -- might ask template root for the style for element and default to DefaultIntrinsicStyles if not provided

            base.Compile(template);
        }

        protected override Type elementType => typeof(UITextElement);

        public override UIElement CreateScoped(TemplateScope inputScope) {
            UIElement retn = new UITextElement();
            retn.children = LightListPool<UIElement>.Get();
            retn.OriginTemplate = this;
            retn.templateContext = new ExpressionContext(inputScope.rootElement, retn);
            return retn;
        }

    }

}