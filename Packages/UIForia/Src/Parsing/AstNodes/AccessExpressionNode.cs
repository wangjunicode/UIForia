using System;
using System.Collections.Generic;
using System.Reflection;

namespace UIForia {

    public class AccessExpressionNodeOld : ExpressionNodeOld {

        private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        public readonly IdentifierNodeOld identifierNodeOld;
        public readonly List<AccessExpressionPartNodeOld> parts;

        public AccessExpressionNodeOld(IdentifierNodeOld identifierNodeOld, List<AccessExpressionPartNodeOld> accessExpressionParts)
            : base(ExpressionNodeType.Accessor) {
            this.identifierNodeOld = identifierNodeOld;
            this.parts = accessExpressionParts;
        }
        
    }

}

