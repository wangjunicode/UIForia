
namespace Src {

    public enum UnaryOperatorType {

        Not,
        Plus,
        Minus

    }
    
    public enum ExpressionNodeType {

        ConstantValue,
        Operator,
        Unary,
        Accessor

    }

    public abstract class ASTNode { }

}