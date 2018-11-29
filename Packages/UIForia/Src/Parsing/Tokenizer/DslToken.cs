using System.Diagnostics;

namespace UIForia {

    [DebuggerDisplay("{value} -> {tokenType}")]
    public struct DslToken {

        public readonly TokenType tokenType;
        public readonly string value;

        public DslToken(TokenType tokenType) {
            this.tokenType = tokenType;
            value = string.Empty;
        }

        public DslToken(TokenType tokenType, string value) {
            this.tokenType = tokenType;
            this.value = value;
        }

        [DebuggerStepThrough]
        public static implicit operator TokenType(DslToken token) {
            return token.tokenType;
        }
        
        [DebuggerStepThrough]
        public static implicit operator string(DslToken token) {
            return token.value;
        }

        public bool IsArithmeticOperator =>
            tokenType == TokenType.Plus ||
            tokenType == TokenType.Minus ||
            tokenType == TokenType.Times ||
            tokenType == TokenType.Divide ||
            tokenType == TokenType.Mod;
        
        public bool IsComparator => 
            tokenType == TokenType.Equals ||
            tokenType == TokenType.NotEquals ||
            tokenType == TokenType.GreaterThan || 
            tokenType == TokenType.GreaterThanEqualTo || 
            tokenType == TokenType.LessThan || 
            tokenType == TokenType.LessThanEqualTo;

        public bool IsBooleanTest =>
            tokenType == TokenType.And ||
            tokenType == TokenType.Or ||
            tokenType == TokenType.Not;
        
        public bool IsOperator =>
            IsArithmeticOperator ||
            IsComparator ||
            IsBooleanTest || 
            tokenType == TokenType.As ||
            tokenType == TokenType.Is ||
            tokenType == TokenType.QuestionMark ||
            tokenType == TokenType.Colon;

        public bool IsUnaryOperator =>
            tokenType == TokenType.Plus ||
            tokenType == TokenType.Minus ||
            tokenType == TokenType.Not;

        public bool UnaryRequiresCheck =>
            tokenType == TokenType.Comma ||
            tokenType == TokenType.Colon ||
            tokenType == TokenType.QuestionMark ||
            tokenType == TokenType.ParenOpen ||
            tokenType == TokenType.ArrayAccessOpen ||
            IsArithmeticOperator ||
            IsComparator;

        public static DslToken Invalid => new DslToken(TokenType.Invalid, string.Empty);

    }

}