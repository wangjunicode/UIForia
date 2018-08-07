using System.Collections.Generic;

namespace Src {
    public abstract class ASTNode { }

    public class ExpressionNode : ASTNode { }

    public abstract class ConstantValueNode : ExpressionNode { }

    public class UnaryExpressionNode : ExpressionNode {
        public string op;
        public ExpressionNode expression;
    }

    public abstract class ExpressionOperand : ASTNode { }

    public class ParenOperatorNode : OperatorNode {
        public ExpressionNode expression;
    }

    public class AccessExpressionPart : ASTNode {
        
    }

    public class PropertyAccessExpression : ExpressionNode {
        private string rootIdentifier;
        public List<AccessExpressionPart> parts;
    }

    public class OperatorExpression : ExpressionNode {
        public ExpressionNode left;
        public ExpressionNode right;
        public OperatorNode op;
    }
    
    public class OperatorNode : ASTNode {
        public OperatorType op;
        public int precedence;
    }

    public class BooleanConstantNode : ConstantValueNode {
        public bool value;
    }

    public class NumericConstantNode : ConstantValueNode {
        public double value;
    }

    public class StringConstantNode : ConstantValueNode {
        public string value;
    }

    public class RootContextLookup : ExpressionNode {
        public IdentifierNode idNode;
    }

    public class IdentifierNode : ASTNode {
        public string identifier;

        public IdentifierNode(string identifier) {
            this.identifier = identifier;
        }
    }

}