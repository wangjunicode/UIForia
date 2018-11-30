using System;
using UIForia.Parsing;

namespace UIForia {

    public class UnaryExpressionNodeOld : ExpressionNodeOld {

        public readonly OperatorType op;
        public readonly ExpressionNodeOld expression;

        public UnaryExpressionNodeOld(ExpressionNodeOld expression, OperatorType op) : base(ExpressionNodeType.Unary) {
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