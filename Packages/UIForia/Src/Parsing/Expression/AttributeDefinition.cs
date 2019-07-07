using System;
using System.Diagnostics;

namespace UIForia.Parsing.Expression {

    [Flags]
    public enum AttributeType {

        Property = 1 << 0,
        Style = 1 << 1,
        Attribute = 1 << 2,
        Event = 1 << 3,
        
        Binding = 1 << 8,
        Const = 1 << 9,
        EnableOnly = 1 << 10

    }
    
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

    
    [DebuggerDisplay("type={attributeType} {key}={value}")]
    public struct AttributeDefinition2 {

        public readonly string key;
        public readonly string value;
        public int line;
        public int column;
        public AttributeType attributeType;

        public AttributeDefinition2(AttributeType attributeType, string key, string value, int line = -1, int column = -1) {
            this.attributeType = attributeType;
            this.key = key;
            this.value = value;
            this.line = line;
            this.column = column;
        }

    }

}