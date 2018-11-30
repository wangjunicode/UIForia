using System;

namespace UIForia {

    public class UnaryExpression_Boolean : Expression<bool> {

        private readonly Expression<bool> expression;

        public UnaryExpression_Boolean(Expression<bool> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            return !(expression.Evaluate(context));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

    public class UnaryExpression_StringBoolean : Expression<bool> {

        private readonly Expression<string> expression;

        public UnaryExpression_StringBoolean(Expression<string> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            return (string.IsNullOrEmpty(expression.Evaluate(context)));
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

    public class UnaryExpression_ObjectBoolean : Expression<bool> {

        private readonly Expression<object> expression;

        public UnaryExpression_ObjectBoolean(Expression<object> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(bool);

        public override bool Evaluate(ExpressionContext context) {
            return expression.Evaluate(context) == null;
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}