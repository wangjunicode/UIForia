using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Bindings;
using UIForia.Elements;
using UIForia.Expressions;
using UIForia.Extensions;
using UIForia.Parsing.Expression;
using UIForia.Util;

namespace UIForia.Templates {

    public class UITextTemplate : UITemplate {
        
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

        public override UIElement CreateScoped(TemplateScope inputScope) {
            UITextElement element = null;
            if (elementType == typeof(UILabelElement)) {
                element = new UILabelElement();
            }
            else if (elementType == typeof(UIParagraphElement)) {
                element = new UIParagraphElement();
            }
            else if (elementType == typeof(UIHeading1Element)) {
                element = new UIHeading1Element();
            }
            else if (elementType == typeof(UIHeading2Element)) {
                element = new UIHeading2Element();
            }
            else if (elementType == typeof(UIHeading3Element)) {
                element = new UIHeading3Element();
            }
            else if (elementType == typeof(UIHeading4Element)) {
                element = new UIHeading4Element();
            }
            else if (elementType == typeof(UIHeading5Element)) {
                element = new UIHeading5Element();
            }
            else if (elementType == typeof(UIHeading6Element)) {
                element = new UIHeading6Element();
            }
            else if (elementType == typeof(UITextElement)) {
                element = new UITextElement();
            }
            else {
                element = Activator.CreateInstance(elementType) as UITextElement;
            }

            Debug.Assert(element != null, nameof(element) + " != null");
            
            element.templateContext = new ExpressionContext(inputScope.rootElement, element);
            element.children = LightList<UIElement>.Get();
            element.OriginTemplate = this;

            return element;
        }

    }

}