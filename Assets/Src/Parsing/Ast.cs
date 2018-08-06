namespace Src {
   
    public class ASTNode { }

    public class ExpressionNode : ASTNode {
        public ExpressionNode left;
        public ExpressionNode right;
    }

    public class LookupExpressionNode : ExpressionNode {
        public readonly string identifier;

        public LookupExpressionNode(string identifier) {
            this.identifier = identifier;
        }
    }

    public class IdentifierNode : ASTNode {
        public readonly string identifier;

        public IdentifierNode(string identifier) {
            this.identifier = identifier;
        }
    }

    public class ConstantExpressionNode : ASTNode {
        public string valueLeft;
    }

    public class ConstantValueNode : ASTNode {
        public string value;
    }
}