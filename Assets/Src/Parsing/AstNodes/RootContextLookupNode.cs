using System;
using System.Diagnostics;
using System.Reflection;

namespace Src {

    [DebuggerDisplay("{idNode.identifier}")]
    public class RootContextLookupNode : ExpressionNode {

        private FieldInfo fieldInfo;
        public readonly IdentifierNode idNode;

        public RootContextLookupNode(IdentifierNode idNode) : base(ExpressionNodeType.RootContextAccessor) {
            this.idNode = idNode;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            if (this.fieldInfo == null) {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                fieldInfo = context.processedType.rawType.GetField(idNode.identifier, flags);
            }
            if (fieldInfo == null) {
                throw new FieldNotDefinedException(context.processedType.rawType, idNode.identifier);
            }
            return fieldInfo.FieldType;
        }

    }

}