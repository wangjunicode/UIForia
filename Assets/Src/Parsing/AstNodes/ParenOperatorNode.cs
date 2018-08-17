using System;

namespace Src {

    public class ParenOperatorNode : ExpressionNode {

        public readonly ExpressionNode expression;
        
        public ParenOperatorNode(ExpressionNode expression) : base (ExpressionNodeType.Paren) {
            this.expression = expression;
        }

        public override bool TypeCheck(ContextDefinition contextDefinition) {
            return expression.TypeCheck(contextDefinition);
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return expression.GetYieldedType(context);
        }

    }

}