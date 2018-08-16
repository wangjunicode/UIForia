
namespace Src {

    public enum UnaryOperatorType {

        Not,
        Plus,
        Minus

    }
    
    public enum ExpressionNodeType {

        LiteralValue,
        Operator,
        Unary,
        RootContextAccessor,
        Accessor,

        Paren,
        ArrayAccess

    }

    public abstract class ASTNode { }

}