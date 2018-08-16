using System;

namespace Src {

    public class RootContextLookup : ExpressionNode {

        public readonly IdentifierNode idNode;

        public RootContextLookup(IdentifierNode idNode) : base (ExpressionNodeType.RootContextAccessor) {
            this.idNode = idNode;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            //context.GetType().GetField(idNode.identifier);
            return null;
        }

    }

}


/*
 * ParseTemplate -> Tokenize Expression -> Parse Expression -> GenerateExpression | GenerateBinding
 * 
 * BindingGenerator
 * IfBindingGenerator
 * StyleBindingGenerator
 * ExpressionGenerator
 *
 * Expression = Function to get some value, returns value
 * Binding = Function to check some value and or set some value, returns void
*/