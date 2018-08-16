namespace Src {

    public class BooleanLiteralExpression : Expression {

        // object type so we only box once
        private readonly object value;

        public BooleanLiteralExpression(bool value) {
            this.value = value;
        }

        public override object Evaluate(TemplateContext context) {
            return value;
        }

    }

}