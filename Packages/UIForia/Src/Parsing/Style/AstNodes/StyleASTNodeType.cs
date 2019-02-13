namespace UIForia.Style.Parsing {

    public enum StyleASTNodeType {

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
        Paren,
        Unit,
        Rgba,
        Rgb,
        Url,
        Property,
        AttributeGroup,
        StateGroup,
        ExpressionGroup,
        Color,
        StyleGroup
    }

}
