using System;

namespace Src {

    public class OperatorExpression_Equality<U, V> : Expression<bool> {

        public readonly Expression<U> left;
        public readonly Expression<V> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_Equality(OperatorType operatorType, Expression<U> left, Expression<V> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool EvaluateTyped(ExpressionContext context) {
            if (operatorType == OperatorType.Equals) {
                return left.EvaluateTyped(context).Equals(right.EvaluateTyped(context));
            }
            else if (operatorType == OperatorType.NotEquals) {
                return !(left.EvaluateTyped(context).Equals(right.EvaluateTyped(context)));
            }
            throw new Exception("Invalid equality operator: " + operatorType);
        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

}