namespace Src {

    public class RepeatTerminalBinding : Binding {

        private readonly RepeatBinding parentBinding;

        public RepeatTerminalBinding(RepeatBinding parentBinding) {
            this.parentBinding = parentBinding;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            parentBinding.Complete(context);
        }

        public override bool IsConstant() {
            return false;
        }

    }

}