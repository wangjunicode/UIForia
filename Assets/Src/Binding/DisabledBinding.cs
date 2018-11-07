namespace UIForia {

    public class DisabledBinding : Binding {

        private readonly Expression<bool> expression;

        public DisabledBinding(Expression<bool> expression) : base("disabled") {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            element.SetEnabled(!expression.EvaluateTyped(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}