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

        public override Type GetYieldedType(ContextDefinition context) {
            MethodInfo info;
            if (yieldedType != null) {
                return yieldedType;
            }
            if (signatureNodeOld.parts.Count == 0) {
                info = context.rootType.GetMethod(identifierNodeOld.identifier, flags);
                if (info == null) {
                    throw new Exception("Method missing");
                }
                yieldedType = info.ReturnType;
                return info.ReturnType;
            }
            Type[] types = new Type[signatureNodeOld.parts.Count];
            for (int i = 0; i < types.Length; i++) {
                types[i] = signatureNodeOld.parts[i].GetYieldedType(context);
            }
            info = context.rootType.GetMethod(identifierNodeOld.identifier, flags, null, types, null);
            if (info == null) {
                throw new Exception("Method missing: " + identifierNodeOld.identifier);
            }

            yieldedType = info.ReturnType;
            return info.ReturnType;
        }

    }

}

