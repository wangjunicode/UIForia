namespace Src {

    public class UnaryBooleanBinding : ExpressionBinding {

        public readonly ExpressionBinding binding;

        public UnaryBooleanBinding(ExpressionBinding binding) {
            this.binding = binding;
        }

        public override object Evaluate(TemplateContext context) {
            object value = binding.Evaluate(context);
            if (value is bool) return !((bool) value);
            return value != null;
        }

    }

}