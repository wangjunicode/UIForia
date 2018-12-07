using System;
using System.Collections.Generic;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia {

    public class UITextTemplate : UITemplate {

        private static readonly TextElementParser textParser = new TextElementParser();
        private readonly Type textElementType;

        public UITextTemplate(Type textElementType, string rawText, List<AttributeDefinition> attributes = null) : base(null, attributes) {
            this.textElementType = textElementType;
            this.RawText = rawText;
        }

        public UITextTemplate(string rawText, List<AttributeDefinition> attributes = null) : base(null, attributes) {
            this.textElementType = null;
            this.RawText = rawText;
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

                Expression<string> expression = template.compiler.Compile<string>(template.rootElementTemplate.RootType, textElementType, expressionParts[i]);
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
            retn.children = ArrayPool<UIElement>.Empty;
            retn.OriginTemplate = this;
            retn.templateContext = new ExpressionContext(inputScope.rootElement, retn);
            return retn;
        }

    }

}