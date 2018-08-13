namespace Src {

    public class UnaryBooleanEvaluator : ExpressionEvaluator {

        public readonly ExpressionEvaluator Evaluator;

        public UnaryBooleanEvaluator(ExpressionEvaluator evaluator) {
            this.Evaluator = evaluator;
        }

        public override object Evaluate(TemplateContext context) {
            object value = Evaluator.Evaluate(context);
            if (value is bool) return !((bool) value);
            return value != null;
        }

    }

}