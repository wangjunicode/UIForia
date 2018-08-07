using System;
using System.Collections.Generic;

namespace Src {

    public class AccessExpressionNode : ExpressionNode {

        public readonly string rootIdentifier;
        public readonly List<AccessExpressionPart> parts;

        public AccessExpressionNode(string identifier, List<AccessExpressionPart> accessExpressionParts)
            : base(ExpressionNodeType.Accessor) {
            this.rootIdentifier = identifier;
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