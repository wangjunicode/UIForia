using System;
using System.Collections.Generic;

namespace Src {

    public class AccessExpressionNode : ExpressionNode {

        public readonly IdentifierNode identifierNode;
        public readonly List<AccessExpressionPartNode> parts;

        public AccessExpressionNode(IdentifierNode identifierNode, List<AccessExpressionPartNode> accessExpressionParts)
            : base(ExpressionNodeType.Accessor) {
            this.identifierNode = identifierNode;
            this.parts = accessExpressionParts;
        }

        public static Type GetYieldedType(ContextDefinition context, AccessExpressionNode expression) {
            return null;
        }

        public override Type GetYieldedType(ContextDefinition context) {
            throw new NotImplementedException();
        }

    }

}