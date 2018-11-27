using System.Collections.Generic;

namespace UIForia {

    public class MethodSignatureNodeOld : ASTNode_Old {

        public readonly IReadOnlyList<ExpressionNodeOld> parts;

        private static readonly IReadOnlyList<ExpressionNodeOld> EmptyParts = new List<ExpressionNodeOld>(0);
        
        public MethodSignatureNodeOld() {
            this.parts = EmptyParts;
        }
        
        public MethodSignatureNodeOld(List<ExpressionNodeOld> parts) {
            this.parts = parts;
        }

    }

}