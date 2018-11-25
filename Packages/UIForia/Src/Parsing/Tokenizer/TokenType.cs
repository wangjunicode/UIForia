using System;

namespace UIForia {

    public enum TokenType {

        ExpressionOpen,
        ExpressionClose,

        // operators
        Plus,
        Minus,
        Times,
        Divide,
        Mod,
        
        Is,
        As,
        Dollar, 
        New, 
        Null,
        Default,
        
        // accessors
        Dot,
        Comma,
        ArrayAccessOpen,
        ArrayAccessClose,
        ParenOpen,
        ParenClose,

        // identifiers
        Identifier,
        Alias,
        At,

        // constants
        String, 
        Boolean,
        Number,

        // booleans
        And,
        Or,
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
    
//        ArithmeticOperator = Plus | Minus | Times | Divide | Mod,
//        Literal = String | Number | Boolean,
//        Comparator = Equals | NotEquals | GreaterThan | GreaterThanEqualTo | LessThan | LessThanEqualTo,
//        BooleanTest = Not | Or | And,
//        AnyIdentifier = Identifier | Alias,
//        UnaryOperator = Plus | Minus | Not,

//        Operator = ArithmeticOperator | QuestionMark | Colon | Comparator | BooleanTest,

//        UnaryRequiresCheck = Comma | Colon | QuestionMark | BooleanTest | ArithmeticOperator | Comparator | ParenOpen | ArrayAccessOpen

    }

}