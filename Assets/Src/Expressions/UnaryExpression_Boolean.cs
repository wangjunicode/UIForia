using System;

namespace Src {

    public class UnaryExpression_Boolean : Expression<bool> {

        private readonly Expression<bool> expression;

        public UnaryExpression_Boolean(Expression<bool> expression) {
            this.expression = expression;
        }

        public override Type YieldedType => typeof(bool);

        public override bool EvaluateTyped(ExpressionContext context) {
            return !(expression.EvaluateTyped(context));
        }

        public override object Evaluate(ExpressionContext context) {
            object value = expression.Evaluate(context);
            if (value is bool) return !((bool) value);
            return value != null;
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

        public override bool EvaluateTyped(ExpressionContext context) {
            return (string.IsNullOrEmpty(expression.EvaluateTyped(context)));
        }

        public override object Evaluate(ExpressionContext context) {
            return (string.IsNullOrEmpty(expression.EvaluateTyped(context)));
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

        public override bool EvaluateTyped(ExpressionContext context) {
            return expression.EvaluateTyped(context) == null;
        }

        public override object Evaluate(ExpressionContext context) {
            return expression.EvaluateTyped(context) == null;
        }

        public override bool IsConstant() {
            return expression.IsConstant();
        }

    }

}