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
                fieldInfo = context.rootType.GetField(idNode.identifier, ReflectionUtil.InstanceBindFlags);
            }
            if (fieldInfo == null) {
                throw new FieldNotDefinedException(context.rootType, idNode.identifier);
            }
            return fieldInfo.FieldType;
        }

    }

}