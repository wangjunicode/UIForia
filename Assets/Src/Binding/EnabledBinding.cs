namespace Src {

    public class EnabledBinding : Binding {

        private readonly Expression<bool> expression;

        // todo -- constant name
        public EnabledBinding(Expression<bool> expression) : base("enabled") {
            this.expression = expression;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            bool enabled = (element.flags & UIElementFlags.Enabled) != 0;
            if (expression.EvaluateTyped(context)) {
                if (enabled) return;
                context.view.EnableElement(element);
            }
            else {
                if (!enabled) return;
                context.view.DisableElement(element);

            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}