using System;

namespace UIForia {

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

        public override T EvaluateTyped(ExpressionContext context) {
            return condition.EvaluateTyped(context) ? left.EvaluateTyped(context) : right.EvaluateTyped(context);
        }

        public override object Evaluate(ExpressionContext context) {
            return condition.EvaluateTyped(context) ? left.Evaluate(context) : right.Evaluate(context);
        }

        public override bool IsConstant() {
            return condition.IsConstant() && left.IsConstant() && right.IsConstant();
        }

    }

}