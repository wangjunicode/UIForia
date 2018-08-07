using System;

namespace Src {

    public class OperatorExpression : ExpressionNode {

        public ExpressionNode left;
        public ExpressionNode right;
        public OperatorNode op;

        public OperatorExpression(ExpressionNode right, ExpressionNode left, OperatorNode op) 
            : base(ExpressionNodeType.Operator) {
            this.left = left;
            this.right = right;
            this.op = op;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            Type typeLeft = left.GetYieldedType(context);
            Type typeRight = right.GetYieldedType(context);
            if (op.op == OperatorType.Plus) {
                if (typeLeft == typeof(string) || typeRight == typeof(string)) {
                    return typeof(string);
                }
            }

            throw new NotImplementedException();
        }

    }

}