namespace Src {

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

        public static implicit operator TokenType(DslToken token) {
            return token.tokenType;
        }
        
        public static implicit operator string(DslToken token) {
            return token.value;
        }
        
    }

}