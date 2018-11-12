using System.Collections.Generic;

namespace UIForia {

    public class MethodSignatureNode : ASTNode {

        public readonly IReadOnlyList<ExpressionNode> parts;

        private static readonly IReadOnlyList<ExpressionNode> EmptyParts = new List<ExpressionNode>(0);
        
        public MethodSignatureNode() {
            this.parts = EmptyParts;
        }
        
        public MethodSignatureNode(List<ExpressionNode> parts) {
            this.parts = parts;
        }

    }

}