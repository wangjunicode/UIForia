using System;
using System.Diagnostics;

namespace UIForia.Parsing.Expressions {

    [Flags]
    public enum AttributeType {

        Context = 1,
        ContextVariable = 1 << 1,
        Alias = 1 << 2,
        Property = 1 << 3,
        Style = 1 << 4,
        Attribute = 1 << 5,
        Event = 1 << 6,
        Conditional = 1 << 7,
        Mouse = 1 << 8,
        Key = 1 << 9,
        Controller = 1 << 10,
        Touch = 1 << 11,
        Slot = 1 << 12,
        SlotContext = 1 << 13,

        Expose = 1 << 14,

        ImplicitVariable = 1 << 15
                           
    }

    [Flags]
    public enum AttributeFlags : ushort {

        Const = 1 << 1,
        EnableOnly = 1 << 2,
        StyleProperty = 1 << 4,
        LateBinding = 1 << 5,
        Sync = 1 << 6

    }
    
    
    // Todo -- remove
    [DebuggerDisplay("{key}={value}")]
    public class AttributeDefinition {

        internal bool isCompiled;
        public readonly string key;
        public readonly string value;
        public int line;
        public int column;

        public AttributeDefinition(string key, string value, int line = -1, int column = -1) {
            this.key = key.Trim();
            this.value = value.Trim();
            this.line = line;
            this.column = column;
        }

    }

    [DebuggerDisplay("type={type} {key}={value}")]
    public struct AttributeDefinition2 {

        public readonly string key;
        public readonly string value;
        public readonly string rawValue;
        public int line;
        public int column;
        public AttributeType type;
        public AttributeFlags flags;

        public AttributeDefinition2(string rawValue, AttributeType type, AttributeFlags flags,  string key, string value, int line = -1, int column = -1) {
            this.rawValue = rawValue;
            this.type = type;
            this.flags = flags;
            this.key = key;
            this.value = value;
            this.line = line;
            this.column = column;
        }

        public string StrippedValue {
            get {
                if (value[0] == '{' && value[value.Length -1] == '}') {
                    return value.Substring(1, value.Length - 2);
                }

                return value;
            }
        }

    }

}