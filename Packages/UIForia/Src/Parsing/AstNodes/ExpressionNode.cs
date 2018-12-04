using System;

namespace UIForia {

    public abstract class ExpressionNodeOld : ASTNode_Old {

        public readonly ExpressionNodeType expressionType;

        protected Type yieldedType;
        
        protected ExpressionNodeOld(ExpressionNodeType expressionType) {
            this.expressionType = expressionType;
        }
        
//        public abstract Type GetYieldedType(ContextDefinition context);

    }

}