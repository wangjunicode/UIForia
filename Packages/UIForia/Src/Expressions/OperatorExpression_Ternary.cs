using System;

namespace UIForia.Expressions {

    public class OperatorExpression_Ternary<T> : Expression<T> {

        public readonly Expression<T> left;
        public readonly Expression<T> right;
        public readonly Expression<bool> condition;

        public OperatorExpression_Ternary(Expression<bool> condition, Expression<T> left, Expression<T> right) {
            this.condition = condition;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(T);

        public override T Evaluate(ExpressionContext context) {
            return condition.Evaluate(context) ? left.Evaluate(context) : right.Evaluate(context);
        }

        public override bool IsConstant() {
            return condition.IsConstant() && left.IsConstant() && right.IsConstant();
        }

    }

}