using System;
using System.Diagnostics;
using System.Reflection;

namespace UIForia {

    [DebuggerDisplay("{idNodeOld.identifier}")]
    public class RootContextLookupNodeOld : ExpressionNodeOld {

        private FieldInfo fieldInfo;
        public readonly IdentifierNodeOld idNodeOld;

        public RootContextLookupNodeOld(IdentifierNodeOld idNodeOld) : base(ExpressionNodeType.RootContextAccessor) {
            this.idNodeOld = idNodeOld;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            if (this.fieldInfo == null) {
                fieldInfo = context.rootType.GetField(idNodeOld.identifier, ReflectionUtil.InstanceBindFlags);
            }
            if (fieldInfo == null) {
                throw new FieldNotDefinedException(context.rootType, idNodeOld.identifier);
            }
            return fieldInfo.FieldType;
        }

    }

}