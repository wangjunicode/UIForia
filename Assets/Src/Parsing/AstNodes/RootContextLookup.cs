using System;

namespace Src {

    public class RootContextLookup : ExpressionNode {

        public readonly IdentifierNode idNode;

        public RootContextLookup(IdentifierNode idNode) : base (ExpressionNodeType.Accessor) {
            this.idNode = idNode;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            //context.GetType().GetField(idNode.identifier);
            return null;
        }

    }

}