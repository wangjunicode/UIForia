namespace Src {

    public class NumericConstantEvaluator : ExpressionEvaluator {

        public readonly double value;

        public NumericConstantEvaluator(double value) {
            this.value = value;
        }

        public double Evaluate(TemplateContext context) {
            return value;
        }

    }

}