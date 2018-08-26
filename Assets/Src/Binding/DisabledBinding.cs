namespace Src {

    public class DisabledBinding : Binding {

        private readonly Expression<bool> expression;
        
        public DisabledBinding(Expression<bool> expression) {
            this.expression = expression;
        }
        
        public override void Execute(UIElement element, UITemplateContext context) {
            bool isDisabled = (element.flags & UIElementFlags.Enabled) == 0;
            if (isDisabled) return;
            if (expression.EvaluateTyped(context)) {
                context.view.DisableElement(element);
            }
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}