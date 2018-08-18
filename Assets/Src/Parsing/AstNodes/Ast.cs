
namespace Src {

    public enum ExpressionNodeType {

        LiteralValue,
        Operator,
        Unary,
        AliasAccessor,
        RootContextAccessor,
        Accessor,
        Paren,
        ArrayAccess,
        MethodCall

    }

    public abstract class ASTNode { }

}