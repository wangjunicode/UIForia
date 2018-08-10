namespace Src {

    public class EnabledBinding : Binding {

        private readonly ExpressionEvaluator getter;

        public EnabledBinding(ExpressionEvaluator getter) {
            this.getter = getter;
        }
        
        public override void Execute(UIElement element, TemplateContext context) {
            bool isVisible = element.isEnabled;
            if (isVisible != (bool) getter.Evaluate(context)) {
                
            }

            context.view.SetEnabled(element, !isVisible);         
        }

    }

}