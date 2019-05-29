using System.Diagnostics;

namespace UIForia.Parsing.Expression {

    [DebuggerDisplay("{key}={value}")]
    public class AttributeDefinition {

        internal bool isCompiled;
        public readonly string key;
        public readonly string value;
        public readonly bool isRealAttribute;
        public int line;
        public int column;

        public AttributeDefinition(string key, string value, int line = -1, int column = -1) {
            this.key = key.Trim();
            this.value = value.Trim();
            this.isRealAttribute = key.StartsWith("x-");
            this.line = line;
            this.column = column;
        }

    }

}