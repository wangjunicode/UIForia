
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

        Paren

    }

    public abstract class ASTNode { }

}