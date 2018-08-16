namespace Src {

    public class UnaryBooleanExpression : Expression {

        private readonly Expression expression;

        public UnaryBooleanExpression(Expression expression) {
            this.expression = expression;
        }

        public override object Evaluate(TemplateContext context) {
            object value = expression.Evaluate(context);
            if (value is bool) return !((bool) value);
            return value != null;
        }
       

    }

}