namespace UIForia.Parsing.Expression {

    public enum ASTNodeType {

        NullLiteral,
        BooleanLiteral,
        NumericLiteral,
        DefaultLiteral,
        StringLiteral,
        Operator,
        TypeOf,
        Identifier,
        DotAccess,
        AccessExpression,
        IndexExpression,
        UnaryNot,
        UnaryMinus,
        BinaryNot,
        DirectCast,
        ListInitializer,
        New,
        Paren

    }

}