using System;

namespace UIForia {

    public class OperatorExpression_Boolean : Expression<bool> {

        public readonly Expression<bool> left;
        public readonly Expression<bool> right;
        public readonly OperatorType operatorType;

        public OperatorExpression_Boolean(OperatorType operatorType, Expression<bool> left, Expression<bool> right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool EvaluateTyped(ExpressionContext context) {
            if (operatorType == OperatorType.And) {
                return left.EvaluateTyped(context) && right.EvaluateTyped(context);
            }
            else if (operatorType == OperatorType.Or) {
                return left.EvaluateTyped(context) || right.EvaluateTyped(context);
            }

            throw new Exception("Invalid boolean operator: " + operatorType);

        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }
    }

}