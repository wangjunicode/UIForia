using System;
using System.Diagnostics;

namespace Src {

    [DebuggerDisplay("{key}={value}")]
    public class AttributeDefinition {

        public bool isCompiled;
        public readonly string key;
        public readonly string value;
        public readonly bool isRealAttribute;

        private static readonly string[] s_BuiltInAttributes = {
            "x-if", "x-show"
        };
        
        public AttributeDefinition(string key, string value) {
            this.key = key.Trim();
            this.value = value.Trim();
            this.isRealAttribute = key.StartsWith("x-") && Array.IndexOf(s_BuiltInAttributes, key) == -1;
        }

    }

}