using System;
using UIForia.Elements;
using UIForia.Expressions;
using UnityEngine;

namespace UIForia.Bindings {

    public class TextBinding_Single : Binding {

        private readonly Expression<string> expression;

        public TextBinding_Single(Expression<string> expression) : base("text") {
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            UITextElement textElement = (UITextElement) element;
            
            try { 
                
                textElement.SetText(expression.Evaluate(context));

            } catch (Exception) {
                
                Debug.Log($"Had a problem evaluating text node. Context: current object is '{context.currentObject}' aux is '{context.aux}' and root object is '{context.rootObject}'");
                
                throw;
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}