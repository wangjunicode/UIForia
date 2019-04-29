using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Bindings;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Parsing.Expression;
using UIForia.Util;

namespace UIForia.Templates {

    public class UITextTemplate : UITemplate {

        private static readonly TextElementParser textParser = new TextElementParser();

        private string unquoted;
        
        public UITextTemplate(Application app, Type textElementType, string rawText, List<AttributeDefinition> attributes = null)
            : base(app, null, attributes) {
            this.elementType = textElementType;
            this.RawText = rawText;
        }

        public UITextTemplate(Application app, string rawText, List<AttributeDefinition> attributes = null)
            : base(app, null, attributes) {
            this.elementType = typeof(UITextElement);
            this.RawText = rawText;
            this.unquoted = rawText.Substring(1, rawText.Length - 1);

        }
        
        protected override Type elementType { get; }

        public string RawText { get; }

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

        public override UIElement CreateScoped(TemplateScope inputScope) {
            UITextElement element = null;
            if (elementType == typeof(UILabelElement)) {
                element = new UILabelElement(RawText);
            }
            else if (elementType == typeof(UIParagraphElement)) {
                element = new UIParagraphElement(RawText);
            }
            else if (elementType == typeof(UIHeading1Element)) {
                element = new UIHeading1Element(RawText);
            }
            else if (elementType == typeof(UIHeading2Element)) {
                element = new UIHeading2Element(RawText);
            }
            else if (elementType == typeof(UIHeading3Element)) {
                element = new UIHeading3Element(RawText);
            }
            else if (elementType == typeof(UIHeading4Element)) {
                element = new UIHeading4Element(RawText);
            }
            else if (elementType == typeof(UIHeading5Element)) {
                element = new UIHeading5Element(RawText);
            }
            else if (elementType == typeof(UIHeading6Element)) {
                element = new UIHeading6Element(RawText);
            }
            else if (elementType == typeof(UITextElement)) {
                element = new UITextElement();
            }
            else {
                element = Activator.CreateInstance(elementType) as UITextElement;
            }

            Debug.Assert(element != null, nameof(element) + " != null");
            
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            element.children = LightListPool<UIElement>.Get();
            element.OriginTemplate = this;

            return element;
        }

    }

}