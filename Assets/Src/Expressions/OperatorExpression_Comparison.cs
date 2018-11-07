using System;

namespace UIForia {

    public class OperatorExpression_Comparison : Expression<bool> {

        public readonly Expression left;
        public readonly Expression right;
        public readonly OperatorType operatorType;

        public OperatorExpression_Comparison(OperatorType operatorType, Expression left, Expression right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);

        public override bool EvaluateTyped(ExpressionContext context) {
            double leftCast = Convert.ToDouble(left.Evaluate(context));
            double rightCast = Convert.ToDouble(right.Evaluate(context));

            switch (operatorType) {
                case OperatorType.GreaterThan:
                    return leftCast > rightCast;

                case OperatorType.GreaterThanEqualTo:
                    return leftCast >= rightCast;

                case OperatorType.LessThan:
                    return leftCast < rightCast;

                case OperatorType.LessThanEqualTo:
                    return leftCast <= rightCast;

                default: throw new Exception("Invalid comparison operator: " + operatorType);
            }

        }

        public override object Evaluate(ExpressionContext context) {
            return EvaluateTyped(context);
        }

        public override bool IsConstant() {
            return left.IsConstant() && right.IsConstant();
        }

    }

}