namespace Src {

    public class StringLiteralExpression : Expression {

        private readonly string value;

        public StringLiteralExpression(string value) {
            this.value = value;
        }

        public override object Evaluate(TemplateContext context) {
            return value;
        }

    }

}