using System;

namespace UIForia {

    public class ParenExpressionNode : ExpressionNode {

        public readonly ExpressionNode expressionNode;
        
        public ParenExpressionNode(ExpressionNode expressionNode) : base (ExpressionNodeType.Paren) {
            this.expressionNode = expressionNode;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return expressionNode.GetYieldedType(context);
        }

    }

}