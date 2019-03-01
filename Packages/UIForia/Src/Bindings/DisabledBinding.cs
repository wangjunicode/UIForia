using UIForia.Elements;
using UIForia.Expressions;

namespace UIForia.Bindings {

    public class DisabledBinding : Binding {

        private readonly Expression<bool> expression;

        public DisabledBinding(Expression<bool> expression) : base("disabled") {
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            element.SetEnabled(!expression.Evaluate(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}