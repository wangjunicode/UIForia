using System;

namespace Src {

    public class ParenExpressionNode : ExpressionNode {

        public readonly ExpressionNode expressionNode;
        
        public ParenExpressionNode(ExpressionNode expressionNode) : base (ExpressionNodeType.Paren) {
            this.expressionNode = expressionNode;
        }

        public override bool TypeCheck(ContextDefinition contextDefinition) {
            return expressionNode.TypeCheck(contextDefinition);
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return expressionNode.GetYieldedType(context);
        }

    }

}