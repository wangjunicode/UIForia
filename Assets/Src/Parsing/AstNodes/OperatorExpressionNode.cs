using System;

namespace Src {

    public class OperatorExpressionNode : ExpressionNode {

        public ExpressionNode left;
        public ExpressionNode right;
        public IOperatorNode op;

        public OperatorExpressionNode(ExpressionNode right, ExpressionNode left, IOperatorNode op) 
            : base(ExpressionNodeType.Operator) {
            this.left = left;
            this.right = right;
            this.op = op;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            Type typeLeft = left.GetYieldedType(context);
            Type typeRight = right.GetYieldedType(context);
            if (op.OpType == OperatorType.Plus) {
                if (typeLeft == typeof(string) || typeRight == typeof(string)) {
                    return typeof(string);
                }
            }

            throw new NotImplementedException();
        }

    }

}