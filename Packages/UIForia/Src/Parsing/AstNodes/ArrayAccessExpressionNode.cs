using System;

namespace UIForia {

    public class ArrayAccessExpressionNode : AccessExpressionPartNode {

        public readonly ExpressionNode expressionNode;

        public ArrayAccessExpressionNode(ExpressionNode expressionNode) : base(ExpressionNodeType.ArrayAccess) {
            this.expressionNode = expressionNode;
        }

        public override bool TypeCheck(ContextDefinition contextDefinition) {
            return typeof(int) == expressionNode.GetYieldedType(contextDefinition);
        }
        
        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(int);
        }

    }

}