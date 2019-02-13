using System;

namespace UIForia {

    public enum ExpressionTokenType {
        
        Invalid,
        
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