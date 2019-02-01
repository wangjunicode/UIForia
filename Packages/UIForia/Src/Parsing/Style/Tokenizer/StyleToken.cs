using System.Diagnostics;

namespace UIForia.Style.Parsing {

    [DebuggerDisplay("{styleTokenType} -> {value}")]
    public struct StyleToken {

        public readonly StyleTokenType styleTokenType;
        public readonly string value;

        public StyleToken(StyleTokenType styleTokenType) {
            this.styleTokenType = styleTokenType;
            value = string.Empty;
        }

        public StyleToken(StyleTokenType styleTokenType, string value) {
            this.styleTokenType = styleTokenType;
            this.value = value;
        }

        [DebuggerStepThrough]
        public static implicit operator StyleTokenType(StyleToken token) {
            return token.styleTokenType;
        }
        
        [DebuggerStepThrough]
        public static implicit operator string(StyleToken token) {
            return token.value;
        }
        
        public static StyleToken Invalid => new StyleToken(StyleTokenType.Invalid, string.Empty);

        public override string ToString() {
            return $"{styleTokenType} -> {value}";
        }
    }

}