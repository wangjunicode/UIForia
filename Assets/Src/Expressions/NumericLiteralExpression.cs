namespace Src {

    public class NumericLiteralExpression : Expression {

        // object type so we only box once
        private readonly object value;

        public NumericLiteralExpression(double value) {
            this.value = value;
        }

        public override object Evaluate(TemplateContext context) {
            return value;
        }

    }

}