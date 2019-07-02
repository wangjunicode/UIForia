namespace UIForia.Parsing.Expression.Tokenizer {

    public enum ExpressionTokenType {
        
        Invalid,
        
        // operators
        Plus,
        Minus,
        Times,
        Divide,
        Mod,
        BinaryOr,
        BinaryAnd,
        BinaryXor,
        BinaryNot,
        
        Is,
        As,
        Dollar, 
        New, 
        Null,
        Default,
        TypeOf,
        
        // accessors
        Dot,
        Comma,
        ExpressionOpen,
        ExpressionClose,
        ArrayAccessOpen,
        ArrayAccessClose,
        ParenOpen,
        ParenClose,

        // identifiers
        Identifier,
//        Alias,
        At,

        // constants
        String, 
        Boolean,
        Number,

        // booleans
        AndAlso,
        OrElse,
        Not,

        // Comparators
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanEqualTo,
        LessThanEqualTo,
        QuestionMark,
        Colon, 
        
        // todo -- use these to make numeric parsing better
        Number_Float,
        Number_Long,
        Number_ULong,
        Number_Decimal,
        Number_Double,
        Number_Byte,
        Number_UInt,

        LambdaArrow,

        Coalesce,

        Elvis

    }

}