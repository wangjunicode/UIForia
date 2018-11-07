using System;

namespace UIForia {

    public class UnaryExpressionNode : ExpressionNode {

        public readonly OperatorType op;
        public readonly ExpressionNode expression;

        public UnaryExpressionNode(ExpressionNode expression, OperatorType op) : base(ExpressionNodeType.Unary) {
            this.expression = expression;
            this.op = op;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            switch (op) {
                case OperatorType.Not:
                    return typeof(bool);
                default:
                    return expression.GetYieldedType(context);
            }
        }

    }

}