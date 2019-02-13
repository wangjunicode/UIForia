using System.Diagnostics;

namespace UIForia {

    [DebuggerDisplay("{key}={value}")]
    public class AttributeDefinition {

        public bool isCompiled;
        public readonly string key;
        public readonly string value;
        public readonly bool isRealAttribute;

        public AttributeDefinition(string key, string value) {
            this.key = key.Trim();
            this.value = value.Trim();
            this.isRealAttribute = key.StartsWith("x-");
        }

    }

}