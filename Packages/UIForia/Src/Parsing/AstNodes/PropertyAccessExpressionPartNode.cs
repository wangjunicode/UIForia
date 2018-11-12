using System;

namespace UIForia {

    public class PropertyAccessExpressionPartNode : AccessExpressionPartNode {

        public readonly string fieldName;

        public PropertyAccessExpressionPartNode(string fieldName) : base (ExpressionNodeType.Accessor) {
            this.fieldName = fieldName;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            throw new NotImplementedException();
        }

    }
    
    public class MethodAccessExpressionPartNode : AccessExpressionPartNode {

        public readonly MethodSignatureNode signatureNode;
        
        public MethodAccessExpressionPartNode(MethodSignatureNode signatureNode) : base (ExpressionNodeType.Accessor) {
            this.signatureNode = signatureNode;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            throw new NotImplementedException();
        }

    }

}