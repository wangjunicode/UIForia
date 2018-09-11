namespace Src {

    public class RepeatChildBinding : Binding {

        private readonly RepeatBinding parentBinding;

        public RepeatChildBinding(RepeatBinding parentBinding) : base("RepeatChild") {
            this.parentBinding = parentBinding;
        }

        public override void Execute(UIElement element, UITemplateContext context) {
            parentBinding.Next(context);
        }

        public override bool IsConstant() {
            return false;
        }

    }

}