using System;

namespace Src {

    public class PropertyAccessExpressionPartNode : AccessExpressionPartNode {

        public readonly string fieldName;

        public PropertyAccessExpressionPartNode(string fieldName) : base (ExpressionNodeType.Accessor) {
            this.fieldName = fieldName;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            throw new NotImplementedException();
        }

    }

}