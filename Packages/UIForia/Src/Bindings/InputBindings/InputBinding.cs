using UIForia.Elements;
using UIForia.Expressions;
using UIForia.UIInput;

namespace UIForia.Bindings.InputBindings {

    public class InputBinding : Binding {

        public new static readonly InputBinding[] EmptyArray = new InputBinding[0];
        
        public readonly InputEventType eventType;
        private readonly Expression<Terminal> expression;

        public InputBinding(InputEventType eventType, Expression<Terminal> expression) : base(eventType.ToString()) {
            this.eventType = eventType;
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            expression.Evaluate(context);
        }

        public override bool IsConstant() {
            return false;
        }

    }
    
}