using System.Diagnostics;

namespace UIForia {

    [DebuggerDisplay("{value} -> {expressionTokenType}")]
    public struct ExpressionToken {

        public readonly ExpressionTokenType expressionTokenType;
        public readonly string value;

        public ExpressionToken(ExpressionTokenType expressionTokenType) {
            this.expressionTokenType = expressionTokenType;
            value = string.Empty;
        }

        public ExpressionToken(ExpressionTokenType expressionTokenType, string value) {
            this.expressionTokenType = expressionTokenType;
            this.value = value;
        }

        [DebuggerStepThrough]
        public static implicit operator ExpressionTokenType(ExpressionToken token) {
            return token.expressionTokenType;
        }
        
        [DebuggerStepThrough]
        public static implicit operator string(ExpressionToken token) {
            return token.value;
        }

        public bool IsArithmeticOperator =>
            expressionTokenType == ExpressionTokenType.Plus ||
            expressionTokenType == ExpressionTokenType.Minus ||
            expressionTokenType == ExpressionTokenType.Times ||
            expressionTokenType == ExpressionTokenType.Divide ||
            expressionTokenType == ExpressionTokenType.Mod;
        
        public bool IsComparator => 
            expressionTokenType == ExpressionTokenType.Equals ||
            expressionTokenType == ExpressionTokenType.NotEquals ||
            expressionTokenType == ExpressionTokenType.GreaterThan || 
            expressionTokenType == ExpressionTokenType.GreaterThanEqualTo || 
            expressionTokenType == ExpressionTokenType.LessThan || 
            expressionTokenType == ExpressionTokenType.LessThanEqualTo;

        public bool IsBooleanTest =>
            expressionTokenType == ExpressionTokenType.And ||
            expressionTokenType == ExpressionTokenType.Or ||
            expressionTokenType == ExpressionTokenType.Not;
        
        public bool IsOperator =>
            IsArithmeticOperator ||
            IsComparator ||
            IsBooleanTest || 
            expressionTokenType == ExpressionTokenType.As ||
            expressionTokenType == ExpressionTokenType.Is ||
            expressionTokenType == ExpressionTokenType.QuestionMark ||
            expressionTokenType == ExpressionTokenType.Colon;

        public bool IsUnaryOperator =>
            expressionTokenType == ExpressionTokenType.Plus ||
            expressionTokenType == ExpressionTokenType.Minus ||
            expressionTokenType == ExpressionTokenType.Not;

        public bool UnaryRequiresCheck =>
            expressionTokenType == ExpressionTokenType.Comma ||
            expressionTokenType == ExpressionTokenType.Colon ||
            expressionTokenType == ExpressionTokenType.QuestionMark ||
            expressionTokenType == ExpressionTokenType.ParenOpen ||
            expressionTokenType == ExpressionTokenType.ArrayAccessOpen ||
            IsArithmeticOperator ||
            IsComparator;

        public static ExpressionToken Invalid => new ExpressionToken(ExpressionTokenType.Invalid, string.Empty);

    }

}