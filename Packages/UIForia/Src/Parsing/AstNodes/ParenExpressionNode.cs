using System;

namespace UIForia {

    public class ParenExpressionNodeOld : ExpressionNodeOld {

        public readonly ExpressionNodeOld expressionNodeOld;
        
        public ParenExpressionNodeOld(ExpressionNodeOld expressionNodeOld) : base (ExpressionNodeType.Paren) {
            this.expressionNodeOld = expressionNodeOld;
        }

    

    }

}