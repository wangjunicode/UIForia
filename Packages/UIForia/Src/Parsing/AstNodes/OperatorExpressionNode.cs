using System;
using UIForia.Parsing;

namespace UIForia {

    public class OperatorExpressionNodeOld : ExpressionNodeOld {

        public ExpressionNodeOld left;
        public ExpressionNodeOld right;
        public object op;

        public OperatorExpressionNodeOld(ExpressionNodeOld right, ExpressionNodeOld left, object op)
            : base(ExpressionNodeType.Operator) {
            this.left = left;
            this.right = right;
            this.op = op;
        }

       
        public OperatorType OpType => default;//op.OpType;

        

    }

}