namespace UIForia {

    public class EnabledBinding : Binding {

        private readonly Expression<bool> expression;

        public EnabledBinding(Expression<bool> expression) : base("enabled") {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            element.SetEnabled(expression.EvaluateTyped(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}