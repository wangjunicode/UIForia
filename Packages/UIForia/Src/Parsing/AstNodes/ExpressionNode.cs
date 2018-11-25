using System;

namespace UIForia {

    public abstract class ExpressionNode : ASTNode {

        public readonly ExpressionNodeType expressionType;

        protected Type yieldedType;
        
        protected ExpressionNode(ExpressionNodeType expressionType) {
            this.expressionType = expressionType;
        }
        
        public abstract Type GetYieldedType(ContextDefinition context);

    }

}