namespace Src {

    public class NumericConstantBinding : ExpressionBinding {

        public readonly double value;

        public NumericConstantBinding(double value) {
            this.value = value;
        }

        public double Evaluate(TemplateContext context) {
            return value;
        }

    }

}