using System;

namespace Src {

    public class ArrayAccessExpressionNode : AccessExpressionPartNode {

        public readonly ExpressionNode expressionNode;

        public ArrayAccessExpressionNode(ExpressionNode expressionNode) : base(ExpressionNodeType.ArrayAccess) {
            this.expressionNode = expressionNode;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            throw new NotImplementedException();
        }

    }

}