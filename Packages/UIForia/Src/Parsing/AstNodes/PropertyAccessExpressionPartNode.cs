using System;

namespace UIForia {

    public class PropertyAccessExpressionPartNodeOld : AccessExpressionPartNodeOld {

        public readonly string fieldName;

        public PropertyAccessExpressionPartNodeOld(string fieldName) : base (ExpressionNodeType.Accessor) {
            this.fieldName = fieldName;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            throw new NotImplementedException();
        }

    }
    
    public class MethodAccessExpressionPartNodeOld : AccessExpressionPartNodeOld {

        public readonly MethodSignatureNodeOld signatureNodeOld;
        
        public MethodAccessExpressionPartNodeOld(MethodSignatureNodeOld signatureNodeOld) : base (ExpressionNodeType.Accessor) {
            this.signatureNodeOld = signatureNodeOld;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            throw new NotImplementedException();
        }

    }

}