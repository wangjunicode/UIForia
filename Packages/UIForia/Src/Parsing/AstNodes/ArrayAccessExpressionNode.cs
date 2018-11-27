using System;

namespace UIForia {

    public class ArrayAccessExpressionNodeOld : AccessExpressionPartNodeOld {

        public readonly ExpressionNodeOld expressionNodeOld;

        public ArrayAccessExpressionNodeOld(ExpressionNodeOld expressionNodeOld) : base(ExpressionNodeType.ArrayAccess) {
            this.expressionNodeOld = expressionNodeOld;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            return typeof(int);
        }

    }

}