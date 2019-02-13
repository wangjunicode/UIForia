namespace UIForia {

    public class EnabledBinding : Binding {

        private readonly Expression<bool> expression;

        public EnabledBinding(Expression<bool> expression) : base("enabled") {
            this.expression = expression;
        }

        public override void Execute(UIElement element, ExpressionContext context) {
            element.SetEnabled(expression.Evaluate(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}