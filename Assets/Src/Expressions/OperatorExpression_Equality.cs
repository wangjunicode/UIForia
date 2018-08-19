using System;

namespace Src {

    public class OperatorExpression_Equality : Expression<bool> {

        public readonly Expression left;
        public readonly Expression right;
        public readonly OperatorType operatorType;
        
        public OperatorExpression_Equality(OperatorType operatorType, Expression left, Expression right) {
            this.operatorType = operatorType;
            this.left = left;
            this.right = right;
        }

        public override Type YieldedType => typeof(bool);
        
        public override bool EvaluateTyped(ExpressionContext context) {
            if (operatorType == OperatorType.Equals) {
                return left.Evaluate(context).Equals(right.Evaluate(context));
            }
            else if (operatorType == OperatorType.NotEquals) {
                return !(left.Evaluate(context).Equals(right.Evaluate(context)));
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