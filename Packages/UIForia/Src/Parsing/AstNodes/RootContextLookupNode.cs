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

     

    }

}