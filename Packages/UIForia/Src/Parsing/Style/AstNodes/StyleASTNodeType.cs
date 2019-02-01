namespace UIForia.Style.Parsing {

    public enum ASTNodeType {

        None,
        NullLiteral,
        BooleanLiteral,
        NumericLiteral,
        DefaultLiteral,
        StringLiteral,
        Operator,
        TypeOf,
        Identifier,
        Invalid,
        DotAccess,
        AccessExpression,
        IndexExpression,
        UnaryNot,
        UnaryMinus,
        DirectCast,
        ListInitializer,
        New,
        Paren

    }

}
