using System;
using System.Diagnostics;
using System.Reflection;

namespace Src {

    [DebuggerDisplay("{identifierNode.identifier}")]
    public class AliasExpressionNode : ExpressionNode {

        public readonly IdentifierNode identifierNode;

        public AliasExpressionNode(IdentifierNode identifierNode) : base(ExpressionNodeType.AliasAccessor) {
            this.identifierNode = identifierNode;
        }
        
        public string alias => identifierNode.identifier;

        public override Type GetYieldedType(ContextDefinition context) {
            return context.ResolveRuntimeAliasType(identifierNode.identifier);
        }

    }

}