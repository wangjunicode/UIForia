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

    }

}