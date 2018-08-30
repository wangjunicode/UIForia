using System;
using Src.Input;

namespace Src.InputBindings {

    public class InputBinding : Binding {

        public new static readonly InputBinding[] EmptyArray = new InputBinding[0];
        
        public readonly InputEventType eventType;
        private readonly Expression<Terminal> expression;

        public InputBinding(InputEventType eventType, Expression<Terminal> expression) {
            this.eventType = eventType;
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            expression.EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return false;
        }

    }
    
}