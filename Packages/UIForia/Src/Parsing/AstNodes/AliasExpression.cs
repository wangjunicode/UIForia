using System;
using System.Diagnostics;
using System.Reflection;

namespace UIForia {

    [DebuggerDisplay("{identifierNodeOld.identifier}")]
    public class AliasExpressionNodeOld : ExpressionNodeOld {

        public readonly IdentifierNodeOld identifierNodeOld;

        public AliasExpressionNodeOld(IdentifierNodeOld identifierNodeOld) : base(ExpressionNodeType.AliasAccessor) {
            this.identifierNodeOld = identifierNodeOld;
        }
        
        public string alias => identifierNodeOld.identifier;

        public override Type GetYieldedType(ContextDefinition context) {
            throw new NotImplementedException();
            // needs to be the return type of the alias resolver
            //context.ResolveRuntimeAliasType(identifierNode.identifier);
        }

    }

}