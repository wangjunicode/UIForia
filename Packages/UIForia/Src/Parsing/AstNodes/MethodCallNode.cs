using System;
using System.Reflection;

namespace UIForia {

    public class MethodCallNodeOld : ExpressionNodeOld {

        private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public readonly IdentifierNodeOld identifierNodeOld;
        public readonly MethodSignatureNodeOld signatureNodeOld;
        
        public MethodCallNodeOld(IdentifierNodeOld identifierNodeOld, MethodSignatureNodeOld signatureNodeOld) : base(ExpressionNodeType.MethodCall) {
            this.identifierNodeOld = identifierNodeOld;
            this.signatureNodeOld = signatureNodeOld;
        }

     

    }

}

