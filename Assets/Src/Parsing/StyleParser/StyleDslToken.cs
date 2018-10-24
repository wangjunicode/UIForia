using System.Diagnostics;

namespace Src.Parsing.StyleParser {

    [DebuggerDisplay("{value} -> {tokenType}")]
    public struct StyleDslToken {

        public readonly StyleTokenType tokenType;
        public readonly string value;

        public StyleDslToken(StyleTokenType tokenType) {
            this.tokenType = tokenType;
            value = string.Empty;
        }

        public StyleDslToken(StyleTokenType tokenType, string value) {
            this.tokenType = tokenType;
            this.value = value;
        }

        [DebuggerStepThrough]
        public static implicit operator StyleTokenType(StyleDslToken token) {
            return token.tokenType;
        }
        
        [DebuggerStepThrough]
        public static implicit operator string(StyleDslToken token) {
            return token.value;
        }
        
    }

}